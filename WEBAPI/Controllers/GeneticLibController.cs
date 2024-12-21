namespace WEBAPI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using GeneticProgrammingLib;
    using System.Text.Json;
    using Contracts;
    using GenProgLibDatabase;

    [ApiController]
    [Route("experiments")]
    public class GeneticLibController : ControllerBase
    {
        [HttpPut]
        public ActionResult<string> Put([FromBody] EvolutionParameters args)
        {
            lock (GPDataBase.used) {
                try {
                    string id = GPDataBase.NewEvolution(args);
                    return id;
                }
                catch (Exception ex) {
                    return StatusCode(500, ex.Message);
                }
            }
        }
        [HttpPost("{id}")]
        public ActionResult<EvolutionResult> Post(string id)
        {
            lock (GPDataBase.used) {
                try {
                    EvolutionResult res = GPDataBase.EvolutionStep(id);
                    return res;
                }
                catch (Exception ex) {
                    if (ex.Message.Contains("id"))
                        return StatusCode(404, ex.Message);
                    else return StatusCode(500, ex.Message);
                }
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<bool> Delete(string id)
        {
            lock (GPDataBase.used) {
                try {
                    return GPDataBase.DeleteEvolution(id);
                }
                catch (Exception ex) {
                    if (ex.Message.Contains("id"))
                        return StatusCode(404, ex.Message);
                    else return StatusCode(500, ex.Message);
                }
            }
        }
    }
}