using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Collections.Generic;


namespace LoggingMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> tmp = args.ToList();
            string dir;
            string nameFilter;
            string serviceName;
            int ageLimit;

            //Help 
            int argH = tmp.IndexOf("-h");
            //Directory
            int argD = tmp.IndexOf("-d");
            //File filter
            int argF = tmp.IndexOf("-f");
            //File age limit
            int argA = tmp.IndexOf("-a");
            //Service name
            int argS = tmp.IndexOf("-s");

            if (argH > -1 || tmp.Count == 0)
            {
                DisplayHelpMessage();
                return;
            }

            Console.WriteLine("===================================================");

            if (argD > -1)
            {
                dir = args[argD + 1];
            }
            else
            {
                Console.WriteLine("Dir must be specified!");
                return;
            }
            if (argF > -1)
            {
                nameFilter = args[argF + 1];
            }
            else
            {
                Console.WriteLine("Name must be specified!");
                return;
            }
            if (argA > -1)
            {
                int.TryParse(args[argA + 1], out ageLimit);
            }
            else
            {
                Console.WriteLine("Age must be specified!");
                return;
            }
            if (argS > -1)
            {
                serviceName = args[argS + 1];
            }
            else
            {
                Console.WriteLine("Service must be specified!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Dir:     " + dir);
            Console.WriteLine("File:    " + nameFilter);
            Console.WriteLine("Service: " + serviceName);
            Console.WriteLine("Age:     " + ageLimit + " days");
            Console.WriteLine();

            TimeSpan writeAge = new TimeSpan();
            DateTime writeTime = new DateTime();
            DateTime maxWriteTime = new DateTime(1900, 01, 01);

            string[] fileList = System.IO.Directory.GetFiles(dir);

            IEnumerable<string> filteredList = fileList.Where(f => f.Contains(nameFilter));

            if (filteredList.Count() == 0)
            {
                Console.WriteLine("No files found matching {0}. Exiting...", nameFilter);
            }
            else
            {
                Console.WriteLine("Files found:");
                foreach (var file in filteredList)
                {
                    Console.WriteLine(file);
                    Console.WriteLine("Modified: " + File.GetLastWriteTime(file));

                    writeTime = File.GetLastWriteTime(file);
                    maxWriteTime = (writeTime > maxWriteTime ? writeTime : maxWriteTime);
                }

                Console.WriteLine();
                Console.WriteLine("Newest write: " + maxWriteTime);

                Console.WriteLine();
                Console.WriteLine(DateTime.Now);

                writeAge = DateTime.Now - maxWriteTime;

                Console.WriteLine("Write age: {3} days, {0} hours, {1} min, {2} sec", writeAge.Hours, writeAge.Minutes, writeAge.Seconds, writeAge.Days);

                if (writeAge.Days >= ageLimit)
                {
                    Console.WriteLine("No writes for "+ageLimit+" day, restarting service " + serviceName);
                    RestartWindowsService(serviceName);
                }
                else
                {
                    Console.WriteLine("File written to recently.");
                }
            }
            Console.ReadKey();
        }

        private static void DisplayHelpMessage()
        {
            Console.WriteLine("Program for monitoring a file matching a name pattern, if the file has a modified time exceeding the specified ");
            Console.WriteLine("number of days the specified service will be restarted.");
            Console.WriteLine("");
            Console.WriteLine("Example:");
            Console.WriteLine(@"LoggingMonitor.exe -d 'D:\Program Files\MyLog' -f MyLogFileName -a 1 -s MyLoggingService");
            Console.WriteLine(@"Will restart service MyLoggingService if no files matching MyLogFileName with modified date 1 day or more are found in D:\Program Files\MyLog");
            Console.WriteLine("");
            Console.WriteLine("Available arguments:");
            Console.WriteLine("-h  - Displays this information");
            Console.WriteLine("-d  - Directory");
            Console.WriteLine("-f  - Name filter");
            Console.WriteLine("-a  - Age filter (days)");
            Console.WriteLine("-s  - Service to restart");
            Console.WriteLine("");
            Console.WriteLine("Author: Viktor Axelsson, Sectra Ltd - 2016-08");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void RestartWindowsService(string serviceName)
        {
            ServiceController serviceController = new ServiceController(serviceName);
            try
            {
                if ((serviceController.Status.Equals(ServiceControllerStatus.Running)) || (serviceController.Status.Equals(ServiceControllerStatus.StartPending)))
                {
                    serviceController.Stop();
                }
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running);
            }
            catch
            {
                Console.WriteLine("Restart failed, service not found?");
            }
        }
    }
}
