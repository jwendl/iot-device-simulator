# IoT Device Simulator for Load Scenarios

## Installing and Running the Simulator

To install the application you can click on the "Deploy to Azure" button below and enter in values for the parameters.

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

After the environment is provisioned, we can go to the Azure Storage Account and upload the required files.
In a folder called 'run' create a main-simulation.json file with a similar definition to the json here.

``` json
[
    {
        "Id": "a2da9567-ff24-4ab8-8b24-795a77139328",
        "DeviceName": "SimulatedFridge-0001",
        "DeviceType": "Fridge",
        "Interval": 5
    }
]
```

Then in a folder called scripts, we would create an example csharp script file (named Fridge.cscript in our example)

``` csharp
class MinMax
{
	[JsonProperty("min")]
	public double Minimum { get; set; }
	[JsonProperty("max")]
	public double Maximum { get; set; }
}

class InitialState
{
	public MinMax Temperature { get; set; }
	public MinMax Humidity { get; set; }
}

class FridgeState
{
	public PreviousState PreviousState { get; set; }
	public CurrentState CurrentState { get; set; }
	public InitialState InitialState { get; set; }
}

class PreviousState
{
	public double Temperature { get; set; }
	public double Humidity { get; set; }
}

class CurrentState
{
	public double Temperature { get; set; }
	public double Humidity { get; set; }
}

double GetRandomNumber(double minimum, double maximum)
{
	var random = new Random();
	return random.NextDouble() * (maximum - minimum) + minimum;
}

var deviceState = JsonConvert.DeserializeObject<FridgeState>(DeviceState);

if (deviceState.PreviousState == null)
{
	deviceState.PreviousState = new PreviousState()
	{
		Temperature = GetRandomNumber(deviceState.InitialState.Temperature.Minimum, deviceState.InitialState.Temperature.Maximum),
		Humidity = GetRandomNumber(deviceState.InitialState.Humidity.Minimum, deviceState.InitialState.Humidity.Maximum),
	};
}

if (deviceState.CurrentState != null)
{
	deviceState.PreviousState.Temperature = deviceState.CurrentState.Temperature;
	deviceState.PreviousState.Humidity = deviceState.CurrentState.Humidity;
}
else
	deviceState.CurrentState = new CurrentState();

var random = new Random();
var oddEven = random.Next(1, 2) % 2 == 0;
if (oddEven)
	deviceState.CurrentState.Temperature = deviceState.PreviousState.Temperature + 1;
else
	deviceState.CurrentState.Temperature = deviceState.PreviousState.Temperature - 1;

oddEven = random.Next(1, 2) % 2 == 0;
if (oddEven)
	deviceState.CurrentState.Humidity = deviceState.PreviousState.Humidity + 1;
else
	deviceState.CurrentState.Humidity = deviceState.PreviousState.Humidity - 1;

var json = JsonConvert.SerializeObject(deviceState.CurrentState);
return json;
```

Then we would create a folder called state that would hold our initial device state (which is used in the above script)

``` json
{
    "initialState": {
      "temperature": {
        "min": 45,
        "max": 70
      },
      "humidity": {
        "min": 10,
        "max": 25
      }
    }
  }
```

Once the files are there, Service Fabric will pick them up and start sending device data to your IoT Hub.

## Configuring and Runing the Simulator

We will need to change a few configuration files in order for the system to connect properly.

The first configuration file is under iot-device-simulator\src\DeviceSimulation\DeviceGenerator\PackageRoot\Config\Settings.xml
This will contain the connection string for our Azure Storage Account.

``` xml
<?xml version="1.0" encoding="utf-8" ?>
<Settings xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/2011/01/fabric">
  <Section Name="ConnectionStrings">
    <Parameter Name="StorageAccountConnectionString" Value="DefaultEndpointsProtocol=https;AccountName=jwiotsolution;AccountKey=...;EndpointSuffix=core.windows.net" />
  </Section>
</Settings>
```

The second configuration file is under iot-device-simulator\src\DeviceSimulation\DeviceSimulator\PackageRoot\Config\Settings.xml
This configuration file contains the value to connect to the IoT Hub that we want our simulated devices to connect to.

``` xml
<?xml version="1.0" encoding="utf-8" ?>
<Settings xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/2011/01/fabric">
  <Section Name="ConnectionStrings">
    <Parameter Name="IoTHubConnectionString" Value="HostName=jwiotsolutionfcdcc.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=..." />
  </Section>

  <Section Name="IoTHub">
    <Parameter Name="HubName" Value="jwiotsolutionfcdcc" />
  </Section>
</Settings>
```

Then the application should run by pressing F5

## Contribution Guidelines

TBD