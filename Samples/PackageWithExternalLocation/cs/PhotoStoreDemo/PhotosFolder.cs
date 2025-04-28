using System.IO;
using System.Reflection;

namespace PhotoStoreDemo
{
    public class PhotosFolder
    {
        public static string Current
        {
            get
            {
                string path = null;
                if (ExecutionMode.IsRunningWithIdentity())
                {
                    path = ExecutionMode.GetSafeAppxLocalFolder();
                }
                if (path == null)
                {
                    path = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
                    path = Path.Combine(path, "Photos");
                    var di = new DirectoryInfo(path);
                    if (!di.Exists)
                    {
                        di.Create();
                    }
                }
                return path;
            }
        }
    }
}
