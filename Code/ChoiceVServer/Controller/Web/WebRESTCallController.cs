using ChoiceVServer.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.Web {
    public class WebRESTCallController : ChoiceVScript {
        public WebRESTCallController() {

        }

        public static string makeWebRESTCall(string url, string method, string data, string contentType, string username, string password) {
            var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(username + ":" + password)));

            var content = new System.Net.Http.StringContent(data, System.Text.Encoding.UTF8, contentType);

            if(method == "GET") {
                var response = client.GetAsync(url).Result;
                return response.Content.ReadAsStringAsync().Result;
            } else if(method == "POST") {
                var response = client.PostAsync(url, content).Result;
                return response.Content.ReadAsStringAsync().Result;
            } else if(method == "PUT") {
                var response = client.PutAsync(url, content).Result;
                return response.Content.ReadAsStringAsync().Result;
            } else if(method == "DEL") {
                var response = client.DeleteAsync(url).Result;
                return response.Content.ReadAsStringAsync().Result;
            } else {
                return null;
            }
        }
    }
}
