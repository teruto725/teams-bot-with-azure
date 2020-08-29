# WordBook for Teams
## 概要
これはTeams上で動作する単語帳botです。
## 機能
この単語帳は以下の機能を持ちます。
### 基本的な単語帳機能
ユーザは以下のコマンドで単語を登録できます。 
- word add "英語" "日本語"  

以下のコマンドで登録単語一覧を確認できます。
- word list  

以下のコマンドで単語のテストができます。出題される単語はユーザがより苦手としている単語から出題されます。
- word test 

出題された問題に対してユーザは答えを直接入力or以下のコマンドで答えを確認します。
- word ans 窓
- word ans : 問題に対して答えを確認できます

答えを直接入力した場合はその成否が返されます。
word ansコマンドを用いた場合は正しい答えが返されます。それに対してユーザは以下のリアクションで回答します。

- 笑い：自分の答えと合致した（正解した）
- びっくり：わからなかった（不正解だった）
- いいね：もう十分復習できたので単語帳から除外する（もう完璧）



### 画像から単語登録機能
このbotは画像から英単語を取得することができます。 この英単語は自動翻訳されその結果を単語帳に追加できます。

以下のコマンドに加えて画像を添付することで画像を読み取ることができます。
- word pic 

検出された単語はtemp wordとして一時的に格納されています。以下のコマンドでtemp wordから単語を一つ取り出すことができます。
- word check pic : 画像から読みとった単語が返されます。

返された単語にたいしてユーザは以下のリアクションをとることができます。

- ステキ（ハート）：対象の単語を単語帳に登録します。
- 怒り：対象の単語を単語帳に登録しない。

### Messenger機能
単語帳が返すメッセージをカスタマイズすることができます。


./teams-file-upload/Files/sample.jsonを参考にしてください

以下のコマンドですでに公開されているMessengerの名前一覧を取得できます。
- messenger list

以下のコマンドでmessengerをセットすることができます。セットすることでメッセージが変化します。
- messenger set "Messengerの名前"

またユーザは自分でMessengerを作成することができます。

1. []にアクセスしてMessengerTemplate.jsonをダウンロードします。
2. ファイルのメッセージ部分を編集します(文字コードがUTF-8にしてください)
3. messenger add "Messengerの名前"　右のコマンドにjsonファイルを添付し送信することでMessengerを登録できます。(「このファイルは既に存在します」というメッセージが出た時は必ず「両方を保持」を選択してください)

### CSV機能
以下のコマンドを打つことで登録単語をcsv出力することができます。
- word tocsv
 
 