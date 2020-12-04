# RabbitMqPostponeConsumer

This is an example how to postpone RabbitMq message by timing, 

in **appsettings.json** you can set **TryCount** as repeat check a message
and **PostponeRateMiliSecond** set the delay time by miliSecond, in every time postpone 
this variable multiplied by trying number.

for example if we set **PostponeRateMiliSecond=10000**
in first postpone, it will have been fired after 10 seconds
in second postpone will have been fired after 20 seconds, and this continue by 30, 40, 50, ...  
when the TryCount is overred, the message is moved to **ExpiredQueue** for creating log.

