# Установка локальных сертификатов

## 1. Установка mkcert

##### Версия для Linux

Сначала нужно установить `certutil`.

```
sudo apt install libnss3-tools
    -или-
sudo yum install nss-tools
    -или-
sudo pacman -S nss
```

затем

```
brew install mkcert
```

или собрать из исходников:

```
go get -u github.com/FiloSottile/mkcert
$(go env GOPATH)/bin/mkcert
```

##### Версия для macOS

```
brew install mkcert
brew install nss # if you use Firefox
```

##### Версия для Windows

Под Windows можно скачать [собранные бинарники](https://github.com/FiloSottile/mkcert/releases) либо воспользоваться одним из пакетных менеджеров: Chocolatey или Scoop.

```
choco install mkcert
    -или-
scoop install mkcertS
```

## 2. Генерация сертификатов

1. Командой: `mkcert -install` добавляем в хранилище доверенных сертификатов компанию СА.

2. Далее генерируем сами ключи командой: `mkcert localhost`
