﻿using GeneticProgrammingLib;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace GeneticLibViewModel
{
    public class MainViewModel : INotifyDataErrorInfo, INotifyPropertyChanged
    {
        Dictionary<string, List<string>> Errors = new Dictionary<string, List<string>>();

        public bool HasErrors => Errors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            if (Errors.ContainsKey(propertyName))
            {
                return Errors[propertyName];

            }
            else
            {
                return Enumerable.Empty<string>();
            }

        }
        public void Validate(string propertyName)
        {
            string results = this[propertyName];

            if (results != "")
            {
                if (Errors.ContainsKey(propertyName))
                    Errors[propertyName][0] = results;
                else
                    Errors.Add(propertyName, new List<string>() { results });
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
            else
            {
                Errors.Remove(propertyName);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
            if (StartCalculation == null) return;
            StartCalculation.RaiseCanExecuteChanged(); ;
            PauseCalculation.RaiseCanExecuteChanged(); ;
            StopCalculation.RaiseCanExecuteChanged(); ;
        }
        public string this[string propertyName]
        {
            get
            {
                string error = string.Empty;
                switch (propertyName)
                {
                    case "Rounds":
                        if (Rounds < 1)
                        {
                            error = "Число раундов должно быть больше 0";
                        }
                        break;
                    case "Players":
                        if (Players < 1 || Players % 2 == 1)
                        {
                            error = "Число игроков должно быть больше 0 и кратно 2";
                        }
                        break;
                    case "Courts":
                        if (Courts < 1)
                        {
                            error = "Число площадок должно быть больше 0";
                        }
                        break;
                    case "PopulationSize":
                        if (PopulationSize < 1)
                        {
                            error = "PopulationSize должно быть больше 0";
                        }
                        break;
                    case "MaxPopulationSize":
                        if (MaxPopulationSize < 1)
                        {
                            error = "MaxPopulationSize должно быть больше 0";
                        }
                        break;
                    case "EvolutionStrength":
                        if (EvolutionStrength > 1 || EvolutionStrength < 0)
                        {
                            error = "EvolutionStrength должно быть в диапазоне [0, 1]";
                        }
                        break;
                    case "MutationRate":
                        if (MutationRate > 1 || MutationRate < 0)
                        {
                            error = "MutationRate должно быть в диапазоне [0, 1]";
                        }
                        break;
                    case "CrossoverRate":
                        if (CrossoverRate > 1 || CrossoverRate < 0)
                        {
                            error = "CrossoverRate должно быть в диапазоне [0, 1]";
                        }
                        break;
                }
                return error;
            }
        }
        public string Error
        {
            get { return Error; }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName = "") { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        private int _Rounds;
        public int Rounds
        {
            get { return _Rounds; }
            set
            {
                _Rounds = value;
                Validate("Rounds");
            }
        }
        private int _Players;
        public int Players
        {
            get { return _Players; }
            set
            {
                _Players = value;
                Validate("Players");
            }
        }
        private int _Courts;
        public int Courts 
        {
            get { return _Courts; }
            set
            {
                _Courts = value;
                Validate("Courts");
            }
        }
        private double _EvolutionStrength;
        public double EvolutionStrength 
        {
            get { return _EvolutionStrength; }
            set
            {
                _EvolutionStrength = value;
                Validate("EvolutionStrength");
            }
        }
        private double _MutationRate;
        public double MutationRate
        {
            get { return _MutationRate; }
            set
            {
                _MutationRate = value;
                Validate("MutationRate");
            }
        }
        private double _CrossoverRate;
        public double CrossoverRate
        {
            get { return _CrossoverRate; }
            set
            {
                _CrossoverRate = value;
                Validate("CrossoverRate");
            }
        }
        private int _PopulationSize;
        public int PopulationSize
        {
            get { return _PopulationSize; }
            set
            {
                _PopulationSize = value;
                Validate("PopulationSize");
            }
        }
        private int _MaxPopulationSize;
        public int MaxPopulationSize
        {
            get { return _MaxPopulationSize; }
            set
            {
                _MaxPopulationSize = value;
                Validate("MaxPopulationSize");
            }
        }
        private AsyncEvolution Evolution { get; set; }
        public string BestRank{ get; set; }
        public Dictionary<int, SortedSet<int>>[] BestTable { get; set; }
        public ActionCommand? StartCalculation { get; private set; }
        public ActionCommand? PauseCalculation { get; private set; }
        public ActionCommand? StopCalculation { get; private set; }
        public string Status {  get; private set; }
        private IUIServices UI;
        public MainViewModel(IUIServices UI)
        {
            this.UI = UI;
            Rounds = 10;
            Players = 10;
            Courts = 10;
            EvolutionStrength = 1.0;
            MutationRate = 0.5;
            CrossoverRate = 0.8;
            PopulationSize = 100;
            MaxPopulationSize = 100;
            Status = "Stopped";
            StartCalculation = new ActionCommand(StartCalculationExecute, StartCalculationCanExecute);
            PauseCalculation = new ActionCommand((x) => 
            { 
                Status = "Paused";
               // RaisePropertyChanged("BestTable");
                StartCalculation.RaiseCanExecuteChanged();
                StopCalculation.RaiseCanExecuteChanged();
                PauseCalculation.RaiseCanExecuteChanged();
            }, (x) => { return Status == "Running"; });
            StopCalculation = new ActionCommand((x) => 
            { 
                Status = "Stopped";
               // RaisePropertyChanged("BestTable");
                StartCalculation.RaiseCanExecuteChanged(); 
                PauseCalculation.RaiseCanExecuteChanged();
                StopCalculation.RaiseCanExecuteChanged();
            }, (x) => { return Status == "Running" || Status == "Paused"; });
        }
        private bool StartCalculationCanExecute(object? o)
        {
            bool res = true;
            string error_list = "";
            string[] ItemsToValidate = new string[8] { "Rounds", "Players", "Courts", "EvolutionStrength", "MutationRate", "CrossoverRate", "PopulationSize", "MaxPopulationSize" };
            foreach (string child in ItemsToValidate)
            {
                if (Errors.ContainsKey(child))
                {
                    foreach (string error in Errors[child])
                        error_list += child + ": " + error + "\n";
                    res = false;
                }
            }
            return res && (Status == "Stopped" || Status == "Paused");
        }
        private void Run()
        {
            if (Status == "Stopped")
                Evolution = new AsyncEvolution(Rounds, Players, Courts, EvolutionStrength, MutationRate, CrossoverRate, PopulationSize, MaxPopulationSize);
            Status = "Running";
            StartCalculation.RaiseCanExecuteChanged();
            PauseCalculation.RaiseCanExecuteChanged();
            StopCalculation.RaiseCanExecuteChanged();
            _ = Task.Factory.StartNew(() =>
            {
                while (Status == "Running")
                {
                    Evolution.Step();
                    var br = Evolution.BestRank();
                    BestRank = $"{br.Item1} : {br.Item2}";
                    BestTable = Evolution.BestTable();
                    RaisePropertyChanged("BestRank");
                }
            });
        }
        public void StartCalculationExecute(object? o)
        {
            try
            {
                Run();
            }
            catch (Exception ex)
            {
                UI.ErrorReport($"Some Error Happened\n{ex.Message}");
            }
        }
    }
}