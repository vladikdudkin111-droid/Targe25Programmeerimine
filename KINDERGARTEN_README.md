# Kindergarten CRUD

Kindergarten CRUD перенесён из подготовленного проекта в `D:\repos\Targe25Programmeerimine`.
Создана и выбрана ветка `kindergarten-crud`; исходные файлы Spaceship сохранены из текущего репозитория.
Сохранены .NET 10, версии пакетов исходного проекта и существующий Spaceship CRUD.
Все новые поясняющие комментарии в C# и Razor написаны на эстонском языке.

## Что добавлено

- `TARge25Shop.Core/Domain/Kindergarten.cs` — сущность со всеми семью полями задания.
- `TARge25Shop.Core/Dto/KindergartenDto.cs` — передача данных между контроллером и сервисом.
- `TARge25Shop.Core/ServiceInterface/IKindergartenServices.cs` — интерфейс сервиса.
- `TARge25Shop.ApplicationServices/Services/KindergartenServices.cs` — создание, чтение, изменение и удаление.
- `TARge25Shop/Models/Kindergarten/` — модели CreateUpdate, Index, Details и Delete.
- `TARge25Shop/Controllers/KindergartenController.cs` — действия CRUD.
- `TARge25Shop/Views/Kindergarten/` — Index, CreateUpdate, Details, Delete.
- `TARge25Shop.Data/Migrations/20260920120000_AddKindergarten.cs` и Designer — новая миграция.

Изменены только четыре существующих файла: `TARge25ShopContext.cs`, `Program.cs`,
`Views/Shared/_Layout.cshtml`, `TARge25ShopContextModelSnapshot.cs`.
Все пути выше указаны относительно папки решения `TARge25Shop`.

Список читается через DbContext, операции над отдельной записью проходят через сервис —
как в исходном Spaceship. Интерфейс использует существующие Bootstrap-стили и английские
названия кнопок. Для создания и редактирования Kindergarten используется общая форма.

`Id` создаётся автоматически как Guid. Имена обязательны, длина — до 100 символов,
`ChildrenCount` — целое неотрицательное число. `CreatedAt` и `UpdatedAt` устанавливаются
на сервере; при редактировании `CreatedAt` сохраняется. Даты используют локальное время
сервера, как существующий Spaceship. Удаление выполняется только после отправки формы
подтверждения. POST-действия защищены antiforgery-проверкой. Отсутствующая запись даёт 404.

## Как открыть и запустить

1. Распаковать архив и открыть `TARge25Shop/Targe25Shop.slnx` в Visual Studio с поддержкой
   .NET 10 и установленным .NET 10 SDK / рабочей нагрузкой ASP.NET.
2. Назначить веб-проект `Targe25Shop` стартовым проектом и восстановить NuGet-пакеты.
3. Проверить `TARge25Shop/appsettings.json`. В исходном проекте используется SQL Server
   LocalDB: `(localdb)\MSSQLLocalDB`, база `TARge25Shop`. LocalDB должен быть установлен.
   При использовании другого SQL Server указать свою строку подключения.
4. Выполнить **Build → Build Solution**.
5. Открыть **Tools → NuGet Package Manager → Package Manager Console** и выполнить:

```powershell
Update-Database -Project TARge25Shop.Data -StartupProject Targe25Shop
```

Миграция уже включена: повторно выполнять `Add-Migration` не нужно.
Для новой базы EF сначала применит исходную `Init`, затем `AddKindergarten`.
Для существующей базы с применённой `Init` добавится только таблица `Kindergartens`.
Не удалять базу и исходную миграцию: это не требуется.

6. Запустить проект через F5 или Ctrl+F5 и открыть пункт **Kindergarten** в меню.

## Проверка перед сдачей

1. Открыть Kindergarten, создать группу: GroupName = Mesilased, ChildrenCount = 18,
   KindergartenName = Päikese lasteaed, TeacherName = Mari Maasikas.
2. Открыть Details: должны отображаться все семь полей, непустой Id и обе даты.
3. Изменить число детей на 19. В Details проверить сохранение Id и CreatedAt,
   обновление ChildrenCount и UpdatedAt.
4. Проверить пустые имена, имя длиннее 100 символов, отрицательное и нецелое число
   детей. Форма должна показать ошибку и не сохранить некорректные данные.
5. Открыть Delete и вернуться через Back to list: запись должна остаться.
   Повторить Delete и подтвердить: запись должна исчезнуть.
6. Проверить Details/Update/Delete с несуществующим Guid: ожидается 404.
7. Проверить создание, просмотр, изменение и удаление тестовой записи Spaceship.

## Git branch, commit и push

Работать нужно в своей существующей локальной копии GitHub-репозитория.
ZIP не содержит историю Git и сам по себе не создаёт ветку на GitHub.

1. В Visual Studio открыть свою локальную копию репозитория и переключиться на `main`.
   Сначала сохранить текущую работу отдельным коммитом, если есть незакоммиченные изменения.
2. **Git → New Branch**: имя `kindergarten-crud`, основа `main`, включить
   **Checkout branch**, нажать **Create**.
3. Перенести из архива новые файлы Kindergarten и четыре изменённых общих файла,
   перечисленных выше. Если в локальной копии есть более новые изменения общих файлов,
   объединить изменения, а не заменять их целиком. Папку `.git` сохранить.
   Если ваш Spaceship новее версии из исходного ZIP, его файлы не заменять.
4. Применить миграцию, собрать проект и пройти проверки выше.
5. **View → Git Changes**. Проверить список: должны быть исходники, представления,
   миграция, Designer и snapshot; не должно быть `.vs`, `bin`, `obj` или файлов базы.
6. Ввести сообщение `Kindergarten CRUD valmis` и нажать **Commit All**
   (или добавить файлы через `+` и нажать **Commit Staged**).
7. Нажать **Push**; при первом push новой ветки Visual Studio может предложить её публикацию.
8. Открыть GitHub, выбрать ветку `kindergarten-crud`, проверить наличие нового коммита.
   Скопировать ссылку именно на эту ветку и отправить преподавателю **в день сдачи**.

Документация Microsoft:
- https://learn.microsoft.com/en-us/visualstudio/version-control/git-create-branch
- https://learn.microsoft.com/en-us/visualstudio/version-control/git-push-remote

## Результаты проверки 20.09.2026

- Сборка решения в D:\repos\Targe25Programmeerimine: .NET SDK 10.0.401,
  0 предупреждений и 0 ошибок. Версии пакетов сохранены (EF Core 10.0.11).
- Entity Framework `migrations has-pending-model-changes`: расхождений нет.
- Миграции Init и AddKindergarten успешно применены к локальной базе TARge25Shop
  на (localdb)\MSSQLLocalDB. Созданы Spaceships и Kindergartens.
- Через запущенный сайт для Kindergarten и Spaceship проверены создание, список,
  Details, Update, подтверждение удаления, удаление и ответ 404 после удаления.
- Kindergarten отклоняет форму с пустыми именами и отрицательным числом детей.
- GET страницы Delete не удаляет запись. POST проверялся с antiforgery-токеном.
- Тестовые записи удалены, проверочный сервер остановлен.
- Git подтверждает: файлы Spaceship и исходной миграции Init не изменены.

Коммит и push ещё не выполнены. Ветка kindergarten-crud уже создана и выбрана;
повторно создавать ветку и переносить файлы по инструкции выше не требуется.