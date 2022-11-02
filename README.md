<a name="readme-top"></a>
<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#license">License</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

<p>This project was built to defend against third parties attempting to compromise an Windows Remote Desktop system that is exposed to the Internet.</p>
<p>In principle it is a Windows Service monitors the Windows Security EventLog looking for failed logon attempts and blocks those that have an unexpected pattern... by default based on particular usernames (*ADMIN* is a favourite) or whether a particular IP has initiated a filed logon multiple times within a time period. Once a pattern is detected it blocks all traffic from that IP address for a set period of time.</p>

<p>The project is C# and was originally built with Visual Studio 2019. It is targetted against Microsoft.NET Framework v4.7.2.</p>

<p><b>Solution contents</b></p>

  <ol>
      <ul>
        <li>ApsDefence - Class library with the defence code</li>
        <li>ApsDefenceHarness - Console application for debugging of the ApsDefence library (Note - debugging must be done with Visual Studio running as ADMIN)</li>
        <li>ApsDefenceService - Windows Service to run the ApsDefence library</li>
      </ul>
  </ol>

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started

This is an example of how you may give instructions on setting up your project locally.
To get a local copy up and running follow these simple example steps.

### Prerequisites

<p>Required Microsoft.NET v4.7.2 - <a href="https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472">download from the Microsoft site</a>

### Installation

1. Clone and build the project (prebuilt code on the <i>TODO</i> list)
2. Copy the binaries for ApsDefenceService to a folder of your choosing `C:\ApsDefenceService` perhaps
3. Start a Command Prompt as Administrator - navigate to the installation folder and execute `InstallService.bat`
4. Execute `services.msc` and validate the Installation of the <b>ApsDefenceService</b>... then START the service.

<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTACT -->
## Contact

Your Name - [@twitter_handle](https://twitter.com/twitter_handle) - email@email_client.com

Project Link: [https://github.com/aphalon/ApsDefence](https://github.com/aphalon/ApsDefence)

<p align="right">(<a href="#readme-top">back to top</a>)</p>
