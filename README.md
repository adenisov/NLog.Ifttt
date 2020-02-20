# NLog.Ifttt
[![Build status](https://ci.appveyor.com/api/projects/status/x3lu48da2ajaep1c?svg=true)](https://ci.appveyor.com/project/adenisov/nlog-ifttt)
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/nlog.ifttt)

## Use case
Imagine you need a way to be notified about emergencies happening on production in a fraction of second. Adding NLog.Ifttt extension to you logging infrastructure will allow to create a seamless integration with the software that is always by the hand.

## Installing
```bash
Install-Package NLog.Ifttt
```
or
```bash
dotnet add NLog.Ifttt
```

1. Add to your NLog.config
```xml
<extensions>
  <add assembly="NLog.Ifttt"/>
</extensions>

<variable name="ifttt_event" value="production_error" />
<variable name="ifttt_key" value="you_key_goes_here" />

<target xsi:type="Ifttt"
                name="ifttt"
                eventName="${ifttt_event}"
                key="${ifttt_key}"
                layout="${layout}"
                value1="${appName}"
                value2="${uppercase:${level}}"
                layoutLocation="Value3">
        </target>

<logger name="notificationLogger" minlevel="Info" writeTo="ifttt" final="true"/>
```

2. Obtain url and key here https://ifttt.com/maker_webhooks/settings
3. Setup Ifttt with a desired way of communication (I prefer [Telegram](https://telegram.org))
4. Once you have something in the NLog logger you will get a message 😉) [Imgur](https://imgur.com/wldMBK3)
5. Enjoy!
