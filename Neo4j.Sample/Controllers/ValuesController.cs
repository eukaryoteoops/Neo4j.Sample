using System.Linq;
using System.Threading.Tasks;
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
            //create if not exist
            _neo4j.Cypher.Merge("(a:Person{name:\"root\"})-[:Child]->(m:Person{name:\"level1\"})")
                .Merge("(m)-[:Child]->(n:Person{name:\"level2\"})")
                .Merge("(m)-[:Child]->(o:Person{name:\"level2-1\"})")
                .Merge("(n)-[:Child]->(p:Person{name:\"level3\"})")
                .Merge("(p)-[:Child]->(q:Person{name:\"level4\"})").ExecuteWithoutResults();
        }

        [HttpGet("level/from/{from}")]
        public IActionResult GetLevel([FromRoute]string from)
        {
            //find all node start from "{from}"
            var others = _neo4j.Cypher
                .OptionalMatch("(n:Person)-[*]->(p:Person)")
                .Where((Person n) => n.Name == from)
                .Return(p => p.As<Person>());
            var self = _neo4j.Cypher
                .OptionalMatch("(n:Person)")
                .Where((Person n) => n.Name == from)
                .Return(n => n.As<Person>());

            var result = others.Results.First() == null ? self.Results : self.Results.Concat(others.Results);
            return Ok(result);
        }

        [HttpPut("add")]
        public async Task<IActionResult> AddNode([FromQuery]string parentName, string name)
        {
            await _neo4j.Cypher
                .OptionalMatch("(a:Person{name:{parentName}})")
                .Merge("(a)-[:Child]->(m:Person{name:{name}})")
                .WithParams(new
                {
                    parentName,
                    name
                }).ExecuteWithoutResultsAsync();
            return Ok();
        }

    }

    public class Person
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
