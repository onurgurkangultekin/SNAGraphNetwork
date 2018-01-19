using Neo4j.Driver.V1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLibrary
{
    public class TieStrength
    {
        private IDriver _driver;
        private const string user = "neo4j";
        private const string pass = "198900";

        List<Tie> managerToUserTakdir;
        List<Tie> managerToUserTesekkur;
        List<Tie> managerToUserDogumGunu;
        List<Tie> managerToUserAll;
        List<Tie> userToUserTakdir;
        List<Tie> userToUserTesekkur;
        List<Tie> userToUserDogumGunu;
        List<Tie> userToUserAll;
        Dictionary<object, object> _depNameList;

        public TieStrength()
        {
            _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "198900"));
            createAllReports();
        }
        public List<Tie> GetReport(string relType, bool manager)
        {
            if (manager)
            {
                if (relType == RelType.TAKDIR)
                {
                    return managerToUserTakdir;
                }
                else if (relType == RelType.TESEKKUR)
                {
                    return managerToUserTesekkur;
                }
                else if (relType == RelType.DOGUMGUNU)
                {
                    return managerToUserDogumGunu;
                }
                else // all
                {
                    return managerToUserAll;
                }
            }
            else // user
            {
                if (relType == RelType.TAKDIR)
                {
                    return userToUserTakdir;
                }
                else if (relType == RelType.TESEKKUR)
                {
                    return userToUserTesekkur;
                }
                else if (relType == RelType.DOGUMGUNU)
                {
                    return userToUserDogumGunu;
                }
                else // all
                {
                    return userToUserAll;
                }
            }
        }
        private void createAllReports()
        {
            _depNameList = getDepartmanNames();
            var relations = getRelations(":TAKDIR", true);
            managerToUserTakdir = createSingleReport(relations);
            relations = getRelations(":TESEKKUR", true);
            managerToUserTesekkur = createSingleReport(relations);
            relations = getRelations(":DOGUMGUNU", true);
            managerToUserDogumGunu = createSingleReport(relations);
            relations = getRelations("", true);
            managerToUserAll = createSingleReport(relations);
            relations = getRelations(":TAKDIR", false);
            userToUserTakdir = createSingleReport(relations);
            relations = getRelations(":TESEKKUR", false);
            userToUserTesekkur = createSingleReport(relations);
            relations = getRelations(":DOGUMGUNU", false);
            userToUserDogumGunu = createSingleReport(relations);
            relations = getRelations("", false);
            userToUserAll = createSingleReport(relations);
        }
        private Dictionary<object, List<object>> getRelations(string relType, bool manager)
        {
            string statement = "match(n{ isManager: {managerToUser}}) - [{relType}]-> (m{ isManager: false}) return n.uID as manager, collect(m.uID) as users";
            statement = statement.Replace("{relType}", relType);
            statement = statement.Replace("{managerToUser}", manager.ToString());
            using (var session = _driver.Session())
            {
                session.Run(statement);
                return session.Run(statement).ToDictionary(x => x.Values["manager"], x => (List<object>)x.Values["users"]);
            }
        }

        private List<Tie> createSingleReport(Dictionary<object, List<object>> relations)
        {
            var tieReport = new List<Tie>();
            for (int i = 0; i < relations.Count; i++)
            {
                var user = relations.Keys.ElementAt(i);
                var users = relations[user];
                var userCounts = new Dictionary<long, long>();

                foreach (var us in users)
                {
                    var u = (long)us;
                    if (!userCounts.ContainsKey(u))
                    {
                        userCounts.Add(u, 1);
                    }
                    else
                    {
                        userCounts[u]++;
                    }
                }
                var max = userCounts.OrderByDescending(x => x.Value).First();
                var min = userCounts.OrderBy(x => x.Value).First();
                tieReport.Add(new Tie
                {
                    UserId = (long)user,
                    DepartmentName = (string)_depNameList[user],
                    MaxTieUserId = max.Key,
                    MaxTieStrength = max.Value,
                    MinTieUserId = min.Key,
                    MinTieStrength = min.Value
                });
            }
            return tieReport;
        }

        private Dictionary<object, object> getDepartmanNames()
        {
            string statement = "match(a) return a.uID as id, a.departmanName as depName order by id";
            using (var session = _driver.Session())
            {
                return session.Run(statement).ToDictionary(x => x.Values["id"], x => x.Values["depName"]);
            }
        }
    }
}
