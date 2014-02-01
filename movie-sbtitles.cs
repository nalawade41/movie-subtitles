using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.IO;
using System.Net;

namespace Subtitler
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"D:\Sherlock.S03E01.HDTV.x264-ChameE";//\Sherlock.S03E01.HDTV.x264-ChameE.mkv";
            string finalPath = string.Empty;
            FileInfo[] fileList = new FileInfo[] { };
            List<string> patterns = new List<string>();
            patterns.Add("*.avi");
            patterns.Add("*.mp4");
            patterns.Add("*.mkv");
            patterns.Add("*.mpg");
            patterns.Add("*.mpeg");
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (string pattern in patterns)
            {
                if (dir.GetFiles(pattern).Count() > 0)
                {
                    fileList = dir.GetFiles(pattern);
                }
            }
            finalPath = fileList.First().FullName;
            foreach (FileInfo file in fileList)
            {
                if (file.Length > new FileInfo(finalPath).Length)
                    finalPath = file.FullName;
            }
            PythonInstance py = new PythonInstance();
            py.CallMethod("somemethod");
            
            var hash = py.CallFunction("get_hash", finalPath);
            string url = "http://api.thesubdb.com/?action=download&hash=" + hash + "&language=en";
            //WebClient client = new WebClient();
            //client.BaseAddress = url;
            //client.Headers.Add("UserAgent", "SubDB/1.0 (subtitle-downloader/1.0; http://github.com/manojmj92/subtitle-downloader)");
            //byte[] data = client.DownloadData(url);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.UserAgent = "SubDB/1.0 (movie-subtitles/1.0; https://github.com/nalawade41/movie-subtitles)";
            WebResponse response = webRequest.GetResponse();
            Console.WriteLine(hash);
            Console.ReadLine();
        }
    }
    public class PythonInstance
    {
        private ScriptEngine engine;
        private ScriptScope scope;
        private ScriptSource source;
        private CompiledCode compiled;
        private object pythonClass;

        public PythonInstance(string className = "PyClass")
        {
            string code = @"
import sys
sys.path.append(r'C:\Program Files\IronPython 2.7\Lib')
import os
import hashlib
import urllib2
class PyClass:
    def __init__(self):
        pass

    def somemethod(self):
        print 'in some method'

    def isodd(self, n):
        return 1 == n % 2

    def get_hash(self,name):
        readsize = 64 * 1024
        with open(name, 'rb') as f:
            size = os.path.getsize(name)
            data = f.read(readsize)
            f.seek(-readsize, os.SEEK_END)
            data += f.read(readsize)
        return hashlib.md5(data).hexdigest()
";
            //creating engine and stuff
            engine = Python.CreateEngine();
            scope = engine.CreateScope();

            //loading and compiling code
            source = engine.CreateScriptSourceFromString(code, Microsoft.Scripting.SourceCodeKind.Statements);
            compiled = source.Compile();

            //now executing this code (the code should contain a class)
            compiled.Execute(scope);

            //now creating an object that could be used to access the stuff inside a python script
            pythonClass = engine.Operations.Invoke(scope.GetVariable(className));
        }

        public void SetVariable(string variable, dynamic value)
        {
            scope.SetVariable(variable, value);
        }

        public dynamic GetVariable(string variable)
        {
            return scope.GetVariable(variable);
        }

        public void CallMethod(string method, params dynamic[] arguments)
        {
            engine.Operations.InvokeMember(pythonClass, method, arguments);
        }

        public dynamic CallFunction(string method, params dynamic[] arguments)
        {
            return engine.Operations.InvokeMember(pythonClass, method, arguments);
        }

    }
}
