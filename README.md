# RabbitMqPostponeConsumer

This is an example how to postpone RabbitMq message by timing, 

in **appsettings.json** you can set **TryCount** as repeat check a message
and **PostponeRateMiliSecond** set the delay time by miliSecond, in every time postpone 
this variable multied by trying number.
when the TryCount is overred, the message is moved to **ExpiredQueue** for creating log.

