﻿// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
namespace MassTransit.AmazonSqsTransport.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Amazon.SimpleNotificationService.Model;


    public static class SnsTransportHeaderExtensions
    {
        public static void Set(this IDictionary<string, MessageAttributeValue> attributes, SendHeaders sendHeaders)
        {
            KeyValuePair<string, object>[] headers = sendHeaders.GetAll()
                .Where(x => x.Value != null && (x.Value is string || x.Value.GetType().GetTypeInfo().IsValueType))
                .ToArray();

            foreach (KeyValuePair<string, object> header in headers)
            {
                if (attributes.ContainsKey(header.Key))
                    continue;

                attributes[header.Key] = new MessageAttributeValue
                {
                    StringValue = header.Value.ToString(),
                    DataType = "String"
                };
            }
        }

        public static void Set(this IDictionary<string, MessageAttributeValue> attributes, string key, string value)
        {
            attributes[key] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = value
            };
        }

        public static void Set(this IDictionary<string, MessageAttributeValue> attributes, string key, Guid? value)
        {
            if (value.HasValue)
            {
                attributes[key] = new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = value.ToString()
                };
            }
        }

        public static void Set(this IDictionary<string, MessageAttributeValue> attributes, string key, TimeSpan? value)
        {
            if (value.HasValue)
            {
                attributes[key] = new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = value.Value.TotalMilliseconds.ToString("F0")
                };
            }
        }
    }
}