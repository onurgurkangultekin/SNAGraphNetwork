using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace GraphLibrary
{
    public class Centrality
    {
        private IDriver _driver;
        private const string user = "neo4j";
        private const string pass = "198900";

        List<User> takdirList;
        List<User> tesekkurList;
        List<User> dogumGunuList;

        public Centrality()
        {
            _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "198900"));
            takdirList = createReport(RelType.TAKDIR);
            tesekkurList = createReport(RelType.TESEKKUR);
            dogumGunuList = createReport(RelType.DOGUMGUNU);
        }

        private Dictionary<object, object> degreeCentrality(string relType)
        {
            string statement = "match(a) optional match (a) - [:{relType}] - (b) return a.uID as id, count(DISTINCT b) as degree order by id";
            statement = statement.Replace("{relType}", relType);
            using (var session = _driver.Session())
            {
                return session.Run(statement).ToDictionary(x => x.Values["id"], x => x.Values["degree"]);
            }
        }

        private Dictionary<object, object> inDegreeCentrality(string relType)
        {
            string statement = "match (a) <- [:{relType}] - (b) return a.uID as id, count(DISTINCT b) as degree";
            statement = statement.Replace("{relType}", relType);
            using (var session = _driver.Session())
            {
                return session.Run(statement).ToDictionary(x => x.Values["id"], x => x.Values["degree"]);
            }
        }

        private Dictionary<object, object> outDegreeCentrality(string relType)
        {
            string statement = "match (a) - [:{relType}] -> (b) return a.uID as id, count(DISTINCT b) as degree";
            statement = statement.Replace("{relType}", relType);
            using (var session = _driver.Session())
            {
                return session.Run(statement).ToDictionary(x => x.Values["id"], x => x.Values["degree"]);
            }
        }

        private Dictionary<object, object> closenessCentrality(string relType)
        {
            //statement = "MATCH (a)-[:{relType}]-(b) WHERE a.uID<>b.uID WITH length(shortestPath((a)-[]-(b))) AS dist, a, b RETURN DISTINCT  a.uID as id, 1.0 / sum(dist)  AS closeness";
            string statement = @"CALL algo.closeness('', '{relType}', {write:true, writeProperty:'closeness'})
                                 YIELD nodes, loadMillis, computeMillis, writeMillis;";

            statement = statement.Replace("{relType}", relType);
            using (var session = _driver.Session())
            {
                session.Run(statement);
                statement = "match(a) return a.uID as id, a.closeness as closeness";
                return session.Run(statement).ToDictionary(x => x.Values["id"], x => x.Values["closeness"]);
            }
        }

        private Dictionary<object, object> betweennessCentrality(string relType)
        {
            //string statement = "MATCH p=allShortestPaths((a)-[:{relType}*]-(b)) where a.uID>b.uID UNWIND nodes(p)[1..-1] as n RETURN n.uID as id, count(*) as betweenness";
            string statement = @"CALL algo.betweenness('','{relType}', {direction:'both',write:true, writeProperty:'betweenness'})
                                 YIELD nodes, minCentrality, maxCentrality, sumCentrality, loadMillis, computeMillis, writeMillis;";
            statement = statement.Replace("{relType}", relType);
            using (var session = _driver.Session())
            {
                session.Run(statement);
                statement = "match(a) return a.uID as id, a.betweenness as betweenness";
                return session.Run(statement).ToDictionary(x => x.Values["id"], x => x.Values["betweenness"]);
            }
        }

        private Dictionary<object, object> eigenvectorCentrality(string relType)
        {
            string statement = "match(a) optional match(a) - [:{relType}] - (b)  return a.uID as id, collect(a.uID) as neigbors order by id";
            statement = statement.Replace("{relType}", relType);
            Dictionary<object, object> dict = new Dictionary<object, object>();
            using (var session = _driver.Session())
            {
                dict = session.Run(statement).ToDictionary(x => x.Values["id"], x => x.Values["neigbors"]);
            }
            long[,] M = new long[dict.Keys.Count, dict.Keys.Count];
            var keyList = dict.Keys.ToList();
            for (int i = 0; i < dict.Count; i++)
            {
                var key = dict.Keys.ElementAt(i);
                List<object> value = (List<object>)dict[dict.Keys.ElementAt(i)];

                foreach (var item in value)
                {
                    var column = keyList.IndexOf(item);
                    M[i, column] += 1;
                }
            }
            var degreeVector = degreeCentrality(relType);
            Dictionary<object, object> eigenCentrality = new Dictionary<object, object>();

            for (int i = 0; i < M.GetLength(0); i++)
            {
                long eigenValue = 0;
                for (int k = 0; k < degreeVector.Values.Count; k++)
                {
                    eigenValue += M[i, k] * (long)degreeVector.Values.ElementAt(k);
                }
                eigenCentrality.Add(degreeVector.Keys.ElementAt(i), eigenValue);
            }
            return eigenCentrality;
        }

        private Dictionary<object, object> getDepartmanNames()
        {
            string statement = "match(a) return a.uID as id, a.departmanName as depName order by id";
            using (var session = _driver.Session())
            {
                return session.Run(statement).ToDictionary(x => x.Values["id"], x => x.Values["depName"]);
            }
        }

        private Dictionary<object, List<object>> getConnectedComponents(string relType)
        {
            string statement = @"CALL algo.unionFind(NULL, '{relType}', {write:true, partitionProperty:'partition'})
                                 YIELD nodes, setCount, loadMillis, computeMillis, writeMillis";
            statement = statement.Replace("{relType}", relType);
            using (var session = _driver.Session())
            {
                session.Run(statement);
                statement = @"MATCH (a) WITH a.partition as partition, count(a.partition) as size
                                 MATCH (b) WHERE b.partition = partition SET b.size = size";
                session.Run(statement);
                statement = "match(a) return a.uID as id, a.partition as partition, a.size as size";
                return session.Run(statement).ToDictionary(x => x.Values["id"], x => new List<object>() { x.Values["partition"], x.Values["size"] });
            }
        }

        private List<User> createReport(string relType)
        {
            Dictionary<object, object> inDegreeList = inDegreeCentrality(relType);
            Dictionary<object, object> outDegreeList = outDegreeCentrality(relType);
            Dictionary<object, object> closenessList = closenessCentrality(relType);
            Dictionary<object, object> betweennessList = betweennessCentrality(relType);
            Dictionary<object, object> eigenvectorList = eigenvectorCentrality(relType);
            Dictionary<object, object> depNameList = getDepartmanNames();
            Dictionary<object, List<object>> componentList = getConnectedComponents(relType);

            List<User> report1 = new List<User>();
            foreach (var item in depNameList)
            {
                inDegreeList.TryGetValue(item.Key, out object inDegreeValue);
                outDegreeList.TryGetValue(item.Key, out object outDegreeValue);
                closenessList.TryGetValue(item.Key, out object closenessValue);
                betweennessList.TryGetValue(item.Key, out object betweennessValue);
                eigenvectorList.TryGetValue(item.Key, out object eigenValue);
                componentList.TryGetValue(item.Key, out List<object> component);
                var row = new User();
                row.UserId = item.Key.As<long>();
                row.DepartmentName = item.Value.As<string>();
                row.InDegreeCentrality = (long)(inDegreeValue != null ? inDegreeValue : 0L);
                row.OutDegreeCentrality = (long)(outDegreeValue != null ? outDegreeValue : 0L);
                row.ClosenessCentrality = (double)(closenessValue != null ? Math.Round((double)closenessValue, 5) : 0d);
                row.BetweennessCentrality = (double)(betweennessValue != null ? betweennessValue : 0d);
                row.EigenvectorCentrality = (long)(eigenValue != null ? eigenValue : 0L);
                row.ComponentNo = component != null && component.Count > 1 ? (long)component[0] : 0L;
                row.ComponentSize = component != null && component.Count > 1 ? (long)component[1] : 0L;

                report1.Add(row);
            }
            return report1;
        }

        public List<User> GetReport(string relType)
        {
            List<User> userList;
            switch (relType)
            {
                case RelType.TAKDIR:
                    userList = takdirList;
                    break;
                case RelType.TESEKKUR:
                    userList = tesekkurList;
                    break;
                case RelType.DOGUMGUNU:
                    userList = dogumGunuList;
                    break;
                default:
                    userList = new List<User>();
                    break;
            }
            return userList;
        }
    }
}