using System;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace VotedDecode
{
    public class DecodeProcess
    {
        public string Server = "";
        public string Token = "";
        public string Key = "";
        public bool Completed = false;
        public string Status = "";

        public bool GetNewKey(out string token,out string key)
        {
            var client = new RestClient(Server);
            var request = new RestRequest("/init/newuser", Method.POST);
            token = "";
            key = "";
            request.AddParameter("userName","");
            request.AddParameter("token", Token);
            IRestResponse response = client.Execute(request);
            var content = response.Content;
            try
            {
                var jo = JObject.Parse(content);
                var jps = jo.Properties().ToDictionary(m => m.Name);
                if (jps.ContainsKey("ret"))
                {
                    var ret = (bool)jps["ret"].Value;
                    if (ret)
                    {
                        if (jps.ContainsKey("publickey"))
                        {
                            key = jps["publickey"].Value.ToString();
                        }
                        if (jps.ContainsKey("token"))
                        {
                            token = jps["token"].Value.ToString();
                        }
                        return true;
                    }
                }
            }
            catch{ }
            return false;
        }
        public string GetList(string token, string key)
        {
            var client = new RestClient(Server);
            var request = new RestRequest("/api/voted/getonionkeys", Method.POST);
            request.AddParameter("publickey", key);
            request.AddParameter("token", token);
            IRestResponse response = client.Execute(request);
            var content = response.Content;
            try
            {
                var jo = JObject.Parse(content);
                var jps = jo.Properties().ToDictionary(m => m.Name);
                if (jps.ContainsKey("ret"))
                {
                    var ret = (bool)jps["ret"].Value;
                    if (ret)
                    {
                        return jps["response"].Value.ToString();
                    }
                    else
                    {
                        return jps["error"].Value.ToString();
                    }
                }
            }
            catch { }
            return "";
        }
        public void Start()
        {
            Completed = false;
            Status = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+ ": Start......";
            Thread td = new Thread(new ThreadStart(Runing));
            td.Start();
        }

        private void Runing()
        {
            if (!Check())
            {
                Completed = true;
                return;
            }
            try
            {
                int rowIndex = 0;
                while (true)
                {
                    var client = new RestClient(Server);
                    var request = new RestRequest("/api/voted/getdecodevoted", Method.POST);
                    request.AddParameter("publickey", Key);
                    request.AddParameter("token", Token);
                    Status = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": Get Decode Voted......";
                    IRestResponse response = client.Execute(request);
                    var content = response.Content;
                    try
                    {
                        var jo = JObject.Parse(content);
                        var jps = jo.Properties().ToDictionary(m => m.Name);
                        if (jps.ContainsKey("ret"))
                        {
                            var ret = (bool)jps["ret"].Value;
                            if (ret)
                            {
                                if (jps.ContainsKey("response"))
                                {
                                    var jo1 = (JObject)jps["response"].Value;
                                    var jps1 = jo1.Properties().ToDictionary(m => m.Name);
                                    if (jps1.ContainsKey("ret") && (bool)jps1["ret"].Value)
                                    {
                                        JArray ja=(JArray)jps1["data"].Value;
                                        if (ja.Count == 0)
                                        {
                                            Status = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": Completed";
                                            break;
                                        }
                                        for (int x = 0; x < ja.Count; x++)
                                        {
                                            rowIndex++;
                                            Status = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": Decode " + rowIndex.ToString() + "......";
                                            var jo2 = (JObject)ja[0];
                                            if (!DecodeVoted(jo2))
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (jps1.ContainsKey("msg"))
                                        {
                                            Status = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": "+jps1["msg"].Value.ToString();
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (jps.ContainsKey("error"))
                                {
                                    Status = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + jps["error"].Value.ToString();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Status = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": response unknown";
                            break;
                        }
                    }
                    catch(Exception ex)
                    {
                        Status = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + ex.Message;
                        break;
                    }
                }
                Status = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": Completed";
                Completed = true;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
                Completed = true;
            }
        }
        private bool DecodeVoted(JObject jo)
        {
            var jps = jo.Properties().ToDictionary(m => m.Name);
            var votingnumber = jps.ContainsKey("votingnumber") ? jps["votingnumber"].Value.ToString() : "";
            var county = jps.ContainsKey("county") ? jps["county"].Value.ToString() : "";
            var onionkey = jps.ContainsKey("onionkey") ? jps["onionkey"].Value.ToString() : "";
            var packages = jps.ContainsKey("packages") ? jps["packages"].Value.ToString() : "";
            var type = jps.ContainsKey("type") ? jps["type"].Value.ToString() : "";
            var encodekey = jps.ContainsKey("encodekey") ? jps["encodekey"].Value.ToString() : "";
            try
            {
                var client = new RestClient(Server);
                var request = new RestRequest("/api/voted/setdecodevoted", Method.POST);
                request.AddParameter("token", Token);
                request.AddParameter("onionkey", onionkey);
                request.AddParameter("votingnumber", votingnumber);
                request.AddParameter("county", county);
                request.AddParameter("packages", packages);
                request.AddParameter("encodekey", encodekey);
                IRestResponse response = client.Execute(request);
                var content = response.Content;
                try
                {
                    var jo1 = JObject.Parse(content);
                    var jps1 = jo1.Properties().ToDictionary(m => m.Name);
                    if (jps1.ContainsKey("ret"))
                    {
                        return (bool)jps1["ret"].Value;
                        
                    }
                    else
                    {
                        Status = "response unknown";
                    }
                }
                catch (Exception ex)
                {
                    Status = ex.Message;
                }
            }
            catch (Exception ex) {
                Status = ex.Message;
            }
            return false;
        }
        private bool Check()
        {
            if (string.IsNullOrEmpty(Key))
            {
                Status = "Missing Key";
                return false;
            }
            if (string.IsNullOrEmpty(Token))
            {
                Status = "Missing Token";
                return false;

            }
            if (string.IsNullOrEmpty(Server))
            {
                Status = "Missing Decode Server";
                return false;

            }
            return true;
        }

    
    }
}
