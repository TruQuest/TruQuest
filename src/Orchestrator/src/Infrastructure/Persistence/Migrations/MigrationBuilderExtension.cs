using System.Reflection;
using System.Text;

using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Persistence.Migrations;

public static class MigrationBuilderExtension
{
    public static void SqlResourceUp(this MigrationBuilder builder, string filename)
    {
        var filenameSplit = filename.Split('.');
        var versionString = filenameSplit[filenameSplit.Length - 2]; // v5[-drop]
        var version = int.Parse(versionString.Substring(1).Split('-').First());
        if (version > 0 && !versionString.EndsWith("-drop"))
        {
            var prevVersionFilename = string.Join(
                '.',
                filenameSplit.SkipLast(2).Concat(new[] { $"v{version - 1}-drop", filenameSplit.Last() })
            );
            builder.SqlResourceUp(prevVersionFilename);
        }

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filename);
        using var ms = new MemoryStream();
        stream!.CopyTo(ms);
        builder.Sql(Encoding.UTF8.GetString(ms.ToArray()));
    }
}
