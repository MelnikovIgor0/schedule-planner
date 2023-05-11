### Перед запуском в файле MainController.cs в 20 строке укажите в HOST адрес хоста (если запускаете локально, ничего не меняйте).

### Подключение БД:
Нужно запустить контейнер с БД и указать правильные настройки подключения.
1. Установить Docker Desktop ([mac](https://docs.docker.com/desktop/install/mac-install/), [windows](https://docs.docker.com/desktop/install/windows-install/), [linux](https://docs.docker.com/desktop/install/linux-install/))
2. В терминале в корневой папке проекта выполните комманду=
```
> docker compose up -d
```
3. Если используете Rider, то в IDE укажите следующие параметры подключения к БД:
   - host/server: **localhost**
   - port: **15432**
   - user: **postgres**
   - password: **123456**
   - database: **schedule-planner**

