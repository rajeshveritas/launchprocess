using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veritas.LaunchProcess.BL;

namespace Veritas.LaunchProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            var sqlConnectionString = ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString;
            Console.Write("Please enter Full path of the CSV file:");
          //  var fileName = Console.ReadLine
            var fileName = @"inputFiles\sampleData.csv";
            var launch = new BL.LaunchProcess(sqlConnectionString,fileName);

            launch.UpdateJNLData();
            Console.ReadLine();
        }
    }                                          
}
