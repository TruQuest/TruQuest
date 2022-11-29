using System.Reflection;
using System.Text;

using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Persistence.Migrations;

public static class MigrationBuilderExtension
{
    public static void SqlResourceUp(this MigrationBuilder builder, string filename)
    {
        var filenameSplit = filename.Split('.');
        var version = int.Parse(filenameSplit[filenameSplit.Length - 2].Substring(1));
        if (version > 0)
        {
            var objectType = filenameSplit[5].TrimEnd('s').ToUpper();
            builder.Sql($@"DROP {objectType} ""{filenameSplit[filenameSplit.Length - 4]}""{(objectType == "TRIGGER" ? $@" ON ""{filenameSplit[6]}""" : string.Empty)};");
        }

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filename);
        using var ms = new MemoryStream();
        stream!.CopyTo(ms);
        builder.Sql(Encoding.UTF8.GetString(ms.ToArray()));
    }

    public static void SqlResourceDown(this MigrationBuilder builder, string filename)
    {
        var filenameSplit = filename.Split('.');
        var version = int.Parse(filenameSplit[filenameSplit.Length - 2].Substring(1));
        var objectType = filenameSplit[5].TrimEnd('s').ToUpper();
        builder.Sql($@"DROP {objectType} ""{filenameSplit[filenameSplit.Length - 4]}""{(objectType == "TRIGGER" ? $@" ON ""{filenameSplit[6]}""" : string.Empty)};");

        if (version > 0)
        {
            using var stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream(filename.Replace($".v{version}.", $".v{version - 1}."));
            using var ms = new MemoryStream();

            stream!.CopyTo(ms);
            builder.Sql(Encoding.UTF8.GetString(ms.ToArray()));
        }
    }
}