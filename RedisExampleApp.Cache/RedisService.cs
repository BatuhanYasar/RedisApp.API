using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisExampleApp.Cache
{
    public class RedisService
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        //Url'yi alıyoruz
        public  RedisService(string url)
        {
            _connectionMultiplexer = ConnectionMultiplexer.Connect(url);
        }

        //Database'yi alıyoruz
        public IDatabase GetDb(int dbIndex) 
        {
          return  _connectionMultiplexer.GetDatabase(dbIndex);
        }
    }
}
