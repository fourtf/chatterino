using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public static class Updates
    {
        public static event EventHandler<UpdateFoundEventArgs> UpdateFound;

        public static void CheckForUpdate(string branchName, VersionNumber currentVersion)
        {
            Task.Run(() =>
            {
                try
                {
                    var request = WebRequest.Create("https://fourtf.com/chatterino/version.json");
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        var parser = new JsonParser();

                        dynamic json = parser.Parse(stream);
                        dynamic branches = json["branches"];

                        foreach (var branch in branches)
                        {
                            if (branchName == (string)branch["name"])
                            {
                                VersionNumber onlineVersion = VersionNumber.Parse(branch["version"]);

                                string url = branch["url"];

                                if (onlineVersion.IsNewerThan(currentVersion))
                                {
                                    UpdateFound?.Invoke(null, new UpdateFoundEventArgs(onlineVersion, url));
                                }

                                break;
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Log("updates"));
                }
            });
        }
    }

    public class UpdateFoundEventArgs : EventArgs
    {
        public VersionNumber Version { get; private set; }
        public string Url { get; private set; }

        public UpdateFoundEventArgs(VersionNumber version, string url)
        {
            Version = version;
            Url = url;
        }
    }
}
