# Invoice System Client (WPF)

C# / WPF で開発した、**Invoice Management System (Lite)** 向けのデスクトップクライアントです。既存の ASP.NET Core Web API を利用し、まずは**ログイン機能**を中心に実装しています。

---

## 概要

本アプリは、既存の **Invoice Management System (Lite)** の API を利用する **WPF クライアント**として作成しています。

Web 版で実装済みの認証基盤・権限制御を活用し、WPF 側では以下を主な目的としています。

- デスクトップアプリとしてのログイン体験の実装
- API 認証（JWT）を利用したクライアント開発
- Admin / Member ロールに応じた画面遷移の土台作成
- 将来的な請求書一覧・会員機能のデスクトップ対応

---

## 背景

元システムは、請求・入金管理業務を題材にした業務向けアプリです。管理者と会員で利用できる機能を分離し、請求・入金・集計を扱う構成になっています。本 WPF クライアントは、その既存システムに対する**別クライアント実装**として作成しています。

---

## 現在の実装範囲

### ✅ 実装済み

- ログイン画面
- メールアドレス / パスワード入力
- API 経由のログイン認証
- ログイン成功時のユーザー情報取得
- ログイン失敗時のエラーメッセージ表示
- `appsettings.json` による API ベース URL 管理
- モダン寄りの暗色 UI ベースのログイン画面
- パスワード表示 / 非表示切替

### 🔲 今後の追加候補

- Admin / Member ロール別画面遷移
- 会員用ダッシュボード
- 請求書一覧
- パスワード再設定画面
- 会員登録画面
- ログアウト機能
- トークン保持の共通管理

---

## 使用 API

ログインには既存バックエンドの `/auth/login` を使用しています。バックエンドは Minimal API で実装されており、`LoginRequest` と `LoginResponse` は以下の形です。

### Request
```json
{
  "email": "admin@example.com",
  "password": "Admin1234!"
}
```

### Response
```json
{
  "id": 1,
  "name": "管理者ユーザー",
  "email": "admin@example.com",
  "role": "Admin",
  "token": "..."
}
```

### ロール

| ロール | 説明 |
|--------|------|
| `Admin` | 管理者 |
| `Member` | 会員 |

---

## 画面イメージ

現在はログイン画面を実装しています。

- 暗色ベース
- 青〜紫系のグラデーション背景
- カード型ログインフォーム
- パスワード表示 / 非表示切替
- エラーメッセージの画面内表示

Web 版ログイン画面の方向性を参考にしつつ、WPF 向けにデスクトップ UI として再構成しています。

---

## 技術スタック

- C# / .NET
- WPF
- HttpClient
- Microsoft.Extensions.Configuration
- JSON ベース設定ファイル（`appsettings.json`）

---

## ディレクトリ構成
```
InvoiceSystem.Wpf/
├─ InvoiceSystem.Wpf.sln
├─ InvoiceSystem.Wpf/
│  ├─ Models/
│  │  ├─ LoginRequest.cs
│  │  ├─ LoginResponse.cs
│  │  └─ CurrentUser.cs
│  ├─ Services/
│  │  └─ AuthService.cs
│  ├─ ViewModels/
│  │  └─ LoginViewModel.cs
│  ├─ Views/
│  │  ├─ LoginWindow.xaml
│  │  └─ LoginWindow.xaml.cs
│  ├─ Helpers/
│  │  └─ RelayCommand.cs
│  ├─ appsettings.json
│  └─ App.xaml
└─ README.md
```

---

## 設定ファイル

API ベース URL は `appsettings.json` で管理しています。
```json
{
  "ApiSettings": {
    "BaseUrl": "https://invoice-naoki-app-api-333afef82093.herokuapp.com/"
  }
}
```

> **備考:** `appsettings.json` は Visual Studio で以下の設定にしてください。
> - ビルド アクション: `Content`
> - 出力ディレクトリにコピー: `新しい場合はコピーする`

---

## 動作確認手順

1. `appsettings.json` に API のベース URL を設定
2. WPF アプリを起動
3. ログイン画面でメールアドレス / パスワードを入力
4. ログイン成功時にユーザー情報を取得
5. 今後はロールに応じて画面遷移を実装予定

---

## デモアカウント

| 区分 | メールアドレス | パスワード |
|------|---------------|-----------|
| 管理者 | admin@example.com | Admin1234! |
| 会員 | member@example.com | Member1234! |

---

## 設計方針

UI と API 通信責務を分離するため、以下のような構成を意識しています。

| レイヤー | 役割 |
|----------|------|
| View | 画面表示・イベント受け取り |
| ViewModel | 入力値、画面状態、コマンド管理 |
| Service | API 通信 |
| Model | Request / Response / CurrentUser などのデータ保持 |

ログイン処理は ViewModel から Service を呼び出し、API から返却された認証結果を `CurrentUser` として保持する方針です。

---

## 今後の改善案

- 成功時ポップアップを廃止し、ロール別画面へ自動遷移
- `SessionService` 導入によるログイン状態管理
- 共通スタイル化（Button / TextBox / Card）
- 会員画面 / 管理者画面の追加
- 請求書一覧・詳細画面の実装
- README へのスクリーンショット追加

---

## 関連システム

本クライアントが接続する元システムは、請求・入金・会員管理を扱う **Invoice Management System (Lite)** です。Web 版では Next.js フロントエンドと ASP.NET Core バックエンドを分離した構成を採用しています。

---

## ライセンス

必要に応じて追記予定。