# CozyBus
CozyBus is a lightweight message bus for your project

[![Build status](https://git.ezlab.ru/codegarage/CozyBus/badges/main/pipeline.svg)](https://git.ezlab.ru/codegarage/CozyBus/)
[![GitHub stars](https://img.shields.io/github/stars/a-legotin/CozyBus.svg)](https://github.com/a-legotin/CozyBus/stargazers)


## Usage RabbitMQ

appsettings.json

    "RabbitOptions": {
        "Url": "localhost",
        "Username": "guest",
        "Password": "guest",
        "Port": 5672
    }

configure services

    var rabbitOptions = new RabbitOptions();
    configuration.GetSection(nameof(RabbitOptions)).Bind(rabbitOptions);
    services.UseRabbitMqMessageBus(builder =>
    {
        builder.WithConnection(rabbitOptions.Url);
        builder.WithUsername(rabbitOptions.Username);
        builder.WithPassword(rabbitOptions.Password);
        builder.WithPort(rabbitOptions.Port);
    });