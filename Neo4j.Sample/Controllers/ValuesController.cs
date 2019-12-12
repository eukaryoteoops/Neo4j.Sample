using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using Newtonsoft.Json;

namespace Neo4j.Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        private readonly IGraphClient _neo4j;

        public ValuesController(IGraphClient neo4j)
        {
            _neo4j = neo4j;
        }

        [HttpGet("level/from/{from}")]
        public IActionResult GetLevel([FromRoute]string from)
        {
            //create if not exist
            _neo4j.Cypher.Merge("(a:Person{name:\"root\"})-[:Child]->(m:Person{name:\"level1\"})")
                .Merge("(m)-[:Child]->(n:Person{name:\"level2\"})")
                .Merge("(m)-[:Child]->(o:Person{name:\"level2-1\"})")
                .Merge("(n)-[:Child]->(p:Person{name:\"level3\"})")
                .Merge("(p)-[:Child]->(q:Person{name:\"level4\"})").ExecuteWithoutResults();
            //find all node start with "{from}"
            var users = _neo4j.Cypher
                .OptionalMatch("(n:Person)-[*]->(p:Person)")
                .Where((Person n) => n.Name == from)
                .Return(p => p.As<Person>());
            var self = _neo4j.Cypher
                .OptionalMatch("(n:Person)")
                .Where((Person n) => n.Name == from)
                .Return(n => n.As<Person>());
            var a = users.Results;
            var result = users.Results.First() == null ? self.Results : self.Results.Union(users.Results);
            return Ok(result);
        }

    }

    public class Person
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
