using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OldWebApp.Helpers
{
    public class ConfigSettingsHelper
    {
        private readonly RegionEndpoint _region;

        public ConfigSettingsHelper(RegionEndpoint region)
        {
            _region = region;
        }

        private async Task<string> GetValueAsync(string parameter)
        {
            var ssmClient = new AmazonSimpleSystemsManagementClient(_region);

            var response = await ssmClient.GetParameterAsync(new GetParameterRequest
            {
                Name = parameter
            });

            return response.Parameter.Value;
        }

        public Task<string> GetAPIURL(string configName)
        {
            return GetValueAsync(configName);
        }
    }
}
