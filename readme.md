## Описание
Репозиторий для демонстрации использования компонентных тестов в среде микросервисов. Задействует http заглушки посредством [проекта mock-server](https://www.mock-server.com/), а авторизационные данные мокируются через утилиту [dotnet user-jwts](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn?view=aspnetcore-7.0&tabs=windows).
Проект написан на C# .NET 7.0.
## Как использовать
Проект полностью готов к использованию через docker-compose:
```
> docker-compose up
```
Для локального запуска необходимо:
* В директории с проектом Shop запустить создание jwt токенов:
```
> dotnet user-jwts create --name Customer --output token
> dotnet user-jwts create --name Accounting --role accounting --output token
```
* Записать выведенные значения в конфигурацию файла `tokens.json` проекта `Shop.ComponentTests`.
* Остановить контейнеры `shop-app` и `shop-tests`:
```
> docker-compose stop shop-app
> docker-compose stop shop-tests
```
* Запустить проект сервиса `Shop`:
```
Shop.exe —-environment Development —-urls http://0.0.0.0:5179 --serviceurls:goods http://localhost:1080 —-serviceurls:orders http://localhost:1080
```
* Теперь проект компонентных тестов `Shop.ComponentTests` можно запускать под debug.
