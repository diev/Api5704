#region License
/*
Copyright 2022-2024 Dmitrii Evdokimov
Open source software

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

namespace Api5704;

public class Config
{
    public string MyThumbprint { get; set; } = "8510d4c1d565f9d071f137cc145e166b3aa71cd9";
    public bool VerboseClient { get; set; } = true;
    public string ServerAddress { get; set; } = "https://ssp.nbki.ru/qbch/";
    public string? ServerThumbprint { get; set; } = "18042E6D06AE9F05B639DF511A8583FEDE72784D";
    public bool ValidateTls { get; set; } = true;
    public bool ValidateThumbprint { get; set; } = true;
    public bool VerboseServer { get; set; } = true;
    public bool UseProxy { get; set; } = false;
    public string? ProxyAddress { get; set; } = "http://192.168.2.1:3128";
    public bool SignFile { get; set; } = true;
    public bool CleanSign { get; set; } = true;
    public string CspTest { get; set; } =
        @"C:\Program Files\Crypto Pro\CSP\csptest.exe";
    public string CspTestSignFile { get; set; } =
        "-sfsign -sign -in %1 -out %2 -my %3 -add -addsigtime";
}
