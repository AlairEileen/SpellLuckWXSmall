using MongoDB.Driver;

namespace Tools.DB
{

    public class MongoDBTool
    {

        /// <summary>
        /// 数据库连接
        /// </summary>
        private const string conn = "mongodb://localhost:27027";


        private const string debugConn = "mongodb://192.168.1.222:27027";
        /// <summary>
        /// 指定的数据库
        /// </summary>
        private const string dbName = "spell_luck_wxxcx";
        /// <summary>
        /// 指定的表
        /// </summary>
        private const string tbName = "table_text";

        /// <summary>
        /// 获取数据库
        /// </summary>
        /// <returns>当前数据库</returns>
        private IMongoDatabase GetMongoDatabase()
        {
            var connectionString = conn;
#if DEBUG
            connectionString = debugConn;
#endif
            MongoClient mongoClient = new MongoClient(connectionString);
            return mongoClient.GetDatabase(dbName);
        }
        /// <summary>
        /// 获取集合
        /// </summary>
        /// <typeparam name="T">集合类型</typeparam>
        /// <returns>该类型集合</returns>
        public IMongoCollection<T> GetMongoCollection<T>()
        {

            string packageName = typeof(T).ToString();
            string collectionName = packageName.Substring(packageName.LastIndexOf(".") + 1);
            return GetMongoDatabase().GetCollection<T>(collectionName);
        }
        /// <summary>
        /// 获取集合
        /// </summary>
        /// <typeparam name="T">集合类型</typeparam>
        /// <param name="collectionName">集合名称</param>
        /// <returns>该类型集合</returns>
        public IMongoCollection<T> GetMongoCollection<T>(string collectionName)
        {
            return GetMongoDatabase().GetCollection<T>(collectionName);
        }
    }
}
