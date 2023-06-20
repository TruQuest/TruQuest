using var f = new FileStream("artifacts/TruQuest.bin", FileMode.Open, FileAccess.Read);
using var r = new StreamReader(f);

using var ff = new FileStream("artifacts/TruQuest-clone.bin", FileMode.CreateNew, FileAccess.Write);
using var w = new StreamWriter(ff);
await w.WriteAsync("**************************************");
await w.WriteAsync(await r.ReadToEndAsync());
await w.WriteAsync("**************************************");
await w.FlushAsync();