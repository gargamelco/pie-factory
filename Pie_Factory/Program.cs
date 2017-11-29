using System;
using System.Threading;
using System.Timers;

    /*
        Pie Factory: pies are made from three components: filling, flavor and topping, each dispensed 
        from a respective hopper with one of these three ingredients.

        Robot Lucy:
        Adds the three ingredients to empty crusts that move on a conveyor belt.
        Can pause the conveyor belt if a ingredient is depleted(изчерпан).

        Robot Joe:
        Fills the hoppers with the respective ingredient.
        Makes sure hoppers are not overfull.
        Makes sure hoppers do not go empty.

        Lucy and Joe as separate threads. 

        Belt speed: one pie crust every 50 ms.
        One pie takes:
        250 gr filling.
        10 gr flavor.
        100 gr topping.
        Every dispensing takes 10 ms.
        Hoppers contain 2 kg material max.
        Each hopper is filled at speed 100 gr / 10 ms.
        Hopper filling start / stop happens immediately.

        Robot Lucy:
        1st – adds filling.
        2nd – adds flavor.
        3rd – adds topping.
        Pauses the conveyor belt, if a hopper does not contain enough ingredient for a successful dispense.
        Resumes the conveyor belt once the missing ingredient is available.

        Robot Joe:
        Fills one hopper at a time.
        Can fill a hopper only partially.

        Implement the factory as a C# program and test it
        Model the hoppers, the robots, and the conveyor belt
        Robots and the belt are serviced by separate threads
    */

namespace Pie_Factory
{
    class Program
    {
        static AutoResetEvent pauseBelt = new AutoResetEvent(false);

        const int beltSpeed = 5000;
        const int dispenseTime = 1000;
        const int hopperMaxAmount = 2000;
        const int hopperFillingSpeed = 1000;

        static Random startingAmount = new Random();
        static int filling = startingAmount.Next(0, 2000);
        static int flavor = startingAmount.Next(0, 2000);
        static int topping = startingAmount.Next(0, 2000);

        static int pieCrust = 0;

        static bool pauseConveyorBelt = false;
        static bool lucyWaitsStopTalk = false;
        static bool lucyPausedBeltStopTalk = false;

        private static object Lock = new object();

        static void RobotLucy(object tag)
        {
            CancellationToken token = (CancellationToken)tag;

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Robot Lucy has been cancelled.");
                    return;
                }

                if (filling < 250 || flavor < 10 || topping < 100)
                {
                    pauseConveyorBelt = true;
                }
                else if (pieCrust < 1)
                {
                    if (!lucyWaitsStopTalk)
                    {
                        pauseConveyorBelt = false;
                        pauseBelt.Set();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lucy is awaiting the next pie crust to appear on the conveyor belt.");
                        lucyWaitsStopTalk = true;
                    }
                    else
                    {
                        pauseConveyorBelt = false;
                        pauseBelt.Set();
                    }
                }
                else
                {
                    lucyWaitsStopTalk = false;

                    lock (Lock)
                    {
                        filling -= 250;
                    }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lucy just dispensed 250 gr of filling for a pie - 1 sec to next dispense");
                        Thread.Sleep(1000);

                    lock (Lock)
                    {
                        flavor -= 10;
                    }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lucy just dispensed 10 gr of flavor for a pie - 1 sec to next dispense");
                        Thread.Sleep(1000);

                    lock (Lock)
                    {
                        topping -= 100;
                    }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lucy just dispensed 100 gr of topping for a pie - 1 sec to use the pie crust");
                        Thread.Sleep(1000);

                    lock (Lock)
                    {
                        pieCrust -= 1;
                    }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lucy just made one pie successfully!");
                    }
                }
            }
        

        static void RobotJoe(object tag)
        {
            CancellationToken token = (CancellationToken)tag;

            int fillingMinAmount = 250;
            int flavorMinAmount = 10;
            int toppingMinAmount = 100;
            int progressInPercentage;

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Robot Joe has been cancelled.");
                    return;
                }

                if (filling < fillingMinAmount)
                {
                    Random partialFillPercentage = new Random();
                    int pfp = partialFillPercentage.Next(50, 100);

                    do
                    {
                        progressInPercentage = filling * 100 / hopperMaxAmount;
                        if (progressInPercentage < pfp)
                        {
                            lock (Lock)
                            {
                                filling += 100;
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Joe is filling the filling ingredient, because of it's insufficient amount.");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Current amount of filling: {filling} gr");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"His progress at loading the filling is {progressInPercentage} % out of {pfp} % which Joe set for himself.");
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Joe has reached his norm with filling the filling ingredient.");
                            break;
                        }
                        
                    }
                    while (progressInPercentage < pfp);
                }

                if (flavor < flavorMinAmount)
                {
                    Random partialFillPercentage = new Random();
                    int pfp = partialFillPercentage.Next(50, 100);

                    do
                    {
                        progressInPercentage = flavor * 100 / hopperMaxAmount;
                        if (progressInPercentage < pfp)
                        {
                            lock (Lock)
                            {
                                flavor += 100;
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Joe is filling the flavor ingredient, because of it's insufficient amount.");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Current amount of flavor: {flavor} gr");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"His progress at loading the flavor is {progressInPercentage} % out of {pfp} % which Joe set for himself.");
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Joe has reached his norm with filling the flavor ingredient.");
                            break;
                        }
                        
                    }
                    while (progressInPercentage < pfp);
                }

                if (topping < toppingMinAmount)
                {
                    Random partialFillPercentage = new Random();
                    int pfp = partialFillPercentage.Next(50, 100);

                    do
                    {
                        progressInPercentage = topping * 100 / hopperMaxAmount;
                        if (progressInPercentage < pfp)
                        {
                            lock (Lock)
                            {
                                topping += 100;
                            }

                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Joe is filling the topping ingredient, because of it's insufficient amount.");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Current amount of topping: {topping} gr");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"His progress at loading the topping is {progressInPercentage} % out of {pfp} % which Joe set for himself.");
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Joe has reached his norm with filling the topping ingredient.");
                            break;
                        }
                    }
                    while (progressInPercentage < pfp);
                }
            }
        }

        static void ConveyorBelt(object tag)
        {
            CancellationToken token = (CancellationToken)tag;

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("ConveyorBelt has been cancelled.");
                    return;
                }

                if (!pauseConveyorBelt)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("A new pie crust was released by the conveyor belt, next in 5 seconds");

                    lock (Lock)
                    {
                        pieCrust += 1;
                    }

                        lucyPausedBeltStopTalk = false;
                        pauseBelt.Set();
                        Thread.Sleep(5000);
                    
                }
                else
                {
                    if (!lucyPausedBeltStopTalk)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Lucy paused temporarily the belt, because of the lack of sufficient amount of some of the ingredients");
                        lucyPausedBeltStopTalk = true;
                        pauseBelt.WaitOne();
                    }
                    else
                    {
                        pauseBelt.WaitOne();
                    }
                }
            }
        }

        private static void RevisionAnnouncer(object source, ElapsedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{Environment.NewLine} *** Current revision of the amounts of the ingredients at {DateTime.Now.ToString("h:mm:ss tt")}. ***");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" *** Filling: {filling} gr., Flavor {flavor} gr., Topping {topping} gr. ***");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" *** Next revision in 10 seconds. ***{Environment.NewLine}");
        }

        static void Main(string[] args)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(RevisionAnnouncer);
            timer.Interval = 10000;
            timer.Enabled = false;

            var belt = new Thread(ConveyorBelt);
            var lucy = new Thread(RobotLucy);
            var joe = new Thread(RobotJoe);

            var cts = new CancellationTokenSource();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" <<< To start The Pie Factory press S. To stop The Pie Factory press X. >>> {Environment.NewLine}");

            while (true)
            {
                try
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (key.KeyChar == 'S')
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"{Environment.NewLine} *** Current revision of the amounts of the ingredients at {DateTime.Now.ToString("h:mm:ss tt")}. ***");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($" *** Filling: {filling} gr., Flavor {flavor} gr., Topping {topping} gr. ***");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($" *** Next revision in 10 seconds. ***{Environment.NewLine}");

                            timer.Enabled = true;

                            belt.Start(cts.Token);
                            lucy.Start(cts.Token);
                            joe.Start(cts.Token);
                        }
                        else if (key.KeyChar == 'X')
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"{Environment.NewLine} <<< Please wait until all threads are cancelled safely! >>> {Environment.NewLine}");

                            timer.Enabled = false;
                            cts.Cancel();

                            belt.Join();
                            lucy.Join();
                            joe.Join();

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"{Environment.NewLine} <<< All threads have been successfully cancelled! >>> {Environment.NewLine}");
                        }
                    }
                }
                catch (ThreadStateException e)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{Environment.NewLine} <<< The program has been terminated, please restart in order to execute again! >>> {Environment.NewLine}");
                }
             }
         }
     }
}
    

