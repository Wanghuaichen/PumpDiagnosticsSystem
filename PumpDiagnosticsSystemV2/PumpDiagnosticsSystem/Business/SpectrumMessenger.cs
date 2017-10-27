﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Timer = System.Timers.Timer;

namespace PumpDiagnosticsSystem.Business
{
    public class SpectrumMessenger
    {
        private const string MsgKey = "Spec1";

        private static readonly ConnectionFactory _factory = new ConnectionFactory() {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            Port = 6012
        };

        public SpectrumMessenger()
        {
            
        }

        public static void Send(string msg)
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel()) {
                channel.QueueDeclare(queue: MsgKey,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(msg);

                channel.BasicPublish(exchange: "",
                                     routingKey: MsgKey,
                                     basicProperties: null,
                                     body: body);
            }
        }

        public static void StartReceive(Action<string> consumerAction)
        {
            Task.Factory.StartNew((() =>
            {
                using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel()) {
                    channel.QueueDeclare(queue: MsgKey,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        consumerAction(message);
                    };
                    channel.BasicConsume(queue: MsgKey,
                        autoAck: true,
                        consumer: consumer);
                }
                Console.ReadLine();
            }));
        }
    }
}