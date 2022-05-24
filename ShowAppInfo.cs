using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHostedCLI;

public static class AppInfo
{
    // https://ascii.co.uk/text - Font: Lean
    static string logoText = Environment.NewLine +
    "     ╔═╗╦╔╦╗╔═╗╦  ╔═╗  ╔═╗╦  ╦" + Environment.NewLine +
    "     ╚═╗║║║║╠═╝║  ║╣   ║  ║  ║" + Environment.NewLine +
    "     ╚═╝╩╩ ╩╩  ╩═╝╚═╝  ╚═╝╩═╝╩" + Environment.NewLine;


    public static void Show()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(logoText);
        Console.ResetColor();

        Console.WriteLine($"     SIMPLE CLI - {Assembly.GetExecutingAssembly().GetName().Version}");

        Console.WriteLine();

        // Console.WriteLine($"OS Version: {Environment.OSVersion}"); // Try this on linux

        // .NET information
        Console.WriteLine($"     Framework: {RuntimeInformation.FrameworkDescription}");

        // OS information
        const string OSRel = "/etc/os-release";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
            File.Exists(OSRel))
        {
            const string PrettyName = "PRETTY_NAME";
            foreach (string line in File.ReadAllLines(OSRel))
            {
                if (line.StartsWith(PrettyName))
                {
                    ReadOnlySpan<char> value = line.AsSpan()[(PrettyName.Length + 2)..^1];
                    Console.WriteLine($"     {value}");
                    break;
                }
            }
        }
        else
        {
            Console.WriteLine($"     {RuntimeInformation.OSDescription}");
        }

        Console.WriteLine();

        const long Mebi = 1024 * 1024;
        const long Gibi = Mebi * 1024;
        GCMemoryInfo gcInfo = GC.GetGCMemoryInfo();

        // Environment information
        Console.WriteLine($"     {nameof(RuntimeInformation.OSArchitecture)}: {RuntimeInformation.OSArchitecture}");
        Console.WriteLine($"     {nameof(Environment.ProcessorCount)}: {Environment.ProcessorCount}");
        Console.WriteLine($"     {nameof(GCMemoryInfo.TotalAvailableMemoryBytes)}: {GetInBestUnit(gcInfo.TotalAvailableMemoryBytes)}");

        // cgroup information
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
            Directory.Exists("/sys/fs/cgroup/cpu") &&
            Directory.Exists("/sys/fs/cgroup/memory"))
        {
            // get cpu cgroup information
            string cpuquota = File.ReadAllLines("/sys/fs/cgroup/cpu/cpu.cfs_quota_us")[0];
            if (int.TryParse(cpuquota, out int quota) &&
                quota > 0)
            {
                Console.WriteLine($"     cfs_quota_us: {quota}");
            }

            // get memory cgroup information
            string usageBytes = File.ReadAllLines("/sys/fs/cgroup/memory/memory.usage_in_bytes")[0];
            string limitBytes = File.ReadAllLines("/sys/fs/cgroup/memory/memory.limit_in_bytes")[0];
            if (long.TryParse(usageBytes, out long usage) &&
                long.TryParse(limitBytes, out long limit) &&
                // above this size is unlikely to be an intentionally constrained cgroup
                limit < 10 * Gibi)
            {
                Console.WriteLine($"     usage_in_bytes: {usageBytes} {GetInBestUnit(usage)}");
                Console.WriteLine($"     limit_in_bytes: {limitBytes} {GetInBestUnit(limit)}");
            }

        }
        Console.WriteLine("/////////////////////////////////////////////");
        Console.WriteLine();
        string GetInBestUnit(long size)
        {
            if (size < Mebi)
            {
                return $"{size} bytes";
            }
            else if (size < Gibi)
            {
                decimal mebibytes = Decimal.Divide(size, Mebi);
                return $"{mebibytes:F} MiB";
            }
            else
            {
                decimal gibibytes = Decimal.Divide(size, Gibi);
                return $"{gibibytes:F} GiB";
            }
        }
    }
}
