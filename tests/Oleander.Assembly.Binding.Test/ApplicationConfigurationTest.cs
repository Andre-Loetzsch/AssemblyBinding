using Oleander.Assembly.Binding.Tool.Xml;

namespace Oleander.Assembly.Binding.Test;

public class ApplicationConfigurationTest
{
    [Fact]
    public void TestFileDoesNotExists()
    {
        var appConfigPath = Path.Combine(AppContext.BaseDirectory, "app.config");
        var appPath = Path.Combine(AppContext.BaseDirectory, "app");

        if (File.Exists(appConfigPath)) File.Delete(appConfigPath); 
        if (!File.Exists(appPath)) File.WriteAllText(appPath, "my application");

        Assert.False(ApplicationConfiguration.IsAppConfigFile(appConfigPath));
    }

    [Fact]
    public void TestInvalidXml()
    {
        var appConfigPath = Path.Combine(AppContext.BaseDirectory, "app1.config");
        var appPath = Path.Combine(AppContext.BaseDirectory, "app1");
        
        if (!File.Exists(appPath)) File.WriteAllText(appPath, "my application");

        Assert.False(ApplicationConfiguration.IsAppConfigFile(appConfigPath));
    }

    [Fact]
    public void TestAppConfig()
    {
        var myAppPath = Path.Combine(AppContext.BaseDirectory, "MyApp");
        var appConfigPath = Path.Combine(AppContext.BaseDirectory, "app2.config");
        var myAppConfigPath = Path.Combine(myAppPath, "app.config");

        if (!Directory.Exists(myAppPath)) Directory.CreateDirectory(myAppPath);
        File.Copy(appConfigPath, myAppConfigPath, true);
        
        Assert.True(ApplicationConfiguration.IsAppConfigFile(myAppConfigPath));
    }

    [Fact]
    public void TestValidConfigurationFile()
    {
        var appConfigPath = Path.Combine(AppContext.BaseDirectory, "app2.exe.config");
        var appPath = Path.Combine(AppContext.BaseDirectory, "app2.exe");
        if (File.Exists(appPath)) File.Delete(appPath);

        Assert.False(ApplicationConfiguration.IsAppConfigFile(appConfigPath));

        File.WriteAllText(appPath, "my application");
        Assert.True(ApplicationConfiguration.IsAppConfigFile(appConfigPath));
    }
}