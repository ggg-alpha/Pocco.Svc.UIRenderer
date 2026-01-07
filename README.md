# Pocco UIRenderer
このリポジトリは、「Pocco」のUIをアプリケーションに届けるためのサービスです。

## 環境
- Linux
- Docker

## 実行方法
1. `.env.sample`を`.env`にコピーして、値を編集する
2. `docker build -t pocco-uirenderer:latest --rm .`
3. `docker run --name pocco-uirenderer --env-file .env -d -p 5099:5099 pocco-uirenderer:latest`
