# PipePuzzles

Казуальная головоломка на Unity: вращай трубы на сетке и собирай непрерывный поток.

![Геймплей PipePuzzles](Docs/gameplay.png)

## Цель игры

**Построить максимально длинный путь.**

Нажимайте на сегменты труб, чтобы поворачивать их. Соединяйте прямые, уголки, тройники и крестовины в одну непрерывную цепочку — чем длиннее заполненный путь, тем выше результат.

## Как играть

1. Тапните по трубе, чтобы повернуть её на 90°.
2. Следите за жёлтым (активным) потоком — он показывает текущий путь.
3. Серые трубы ещё не входят в цепочку: поверните их так, чтобы продлить поток.
4. Чем длиннее непрерывный путь, тем больше очков.

## Типы труб

| Тип | Описание |
|-----|----------|
| **I** | Прямая — соединяет противоположные стороны |
| **L** | Уголок — поворот на 90° |
| **T** | Тройник — три направления |
| **X** | Крест — все четыре стороны |

## Стек

- Unity 6
- C#
- Zenject, UniTask, DOTween, LeanPool

## Web-версия (GitHub Pages)

Готовый WebGL-билд лежит в папке [`docs/`](docs/) — её можно публиковать через GitHub Pages (**Settings → Pages → Deploy from a branch → `/docs`**).

### Локальная проверка

```bash
./scripts/serve-webgl.sh
```

Откройте http://127.0.0.1:8080/ в браузере (другой порт: `./scripts/serve-webgl.sh 9090`).

> Не открывайте `docs/index.html` напрямую через `file://` — WebGL так не запустится.

### Пересборка

В Unity: **Build → WebGL for GitHub Pages**, либо из терминала:

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -quit -batchmode -nographics \
  -projectPath "$(pwd)" \
  -buildTarget WebGL \
  -executeMethod Pipes.Editor.WebGLPagesBuilder.BuildFromCommandLine \
  -logFile Logs/webgl-build.log
```

После пуша включите Pages на ветку `main`, папка `/docs`. Игра будет по адресу:

`https://MaxDeMaW.github.io/PipePuzzles/`

## Лицензия

См. [LICENSE](LICENSE).
