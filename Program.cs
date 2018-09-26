﻿using System;
using System.Threading;
using System.Net.Http;
using Screen;
using System.Text;

namespace GoldenEyeRecall
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new ScreenReader();

            while (true)
            {
                if (!p.IsLoading())
                {
                    PlayVid();
                }
                else
                {
                    ResetVid();
                }

                Thread.Sleep(56);
            }
        }

        static async void PlayVid()
        {
            string commandUrl = "http://127.0.0.1:8080/requests/status.xml?command=pl_play";
            var byteArray = Encoding.ASCII.GetBytes(":terryjr");
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                HttpResponseMessage res = await client.GetAsync(commandUrl);
            }
        }

        static async void ResetVid()
        {
            string pauseUrl = "http://127.0.0.1:8080/requests/status.xml?command=pl_pause";
            string resetUrl = "http://127.0.0.1:8080/requests/status.xml?command=seek&val=0";
            var byteArray = Encoding.ASCII.GetBytes(":terryjr");
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                HttpResponseMessage res = await client.GetAsync(pauseUrl);
                HttpResponseMessage res2 = await client.GetAsync(resetUrl);
            }
        }
    }
}
