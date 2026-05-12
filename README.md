# Tugas Besar IF25-21013 Strategi Algoritma

## Pemanfaatan Algoritma Greedy dalam Pembuatan Bot Permainan Robocode Tank Royale

## Penjelasan Bot

| Status | Nama Bot | Lokasi Folder | Versi | Versi Tag | Algoritma Greedy |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Bot Utama** | **Orion** | `src/main-bot/Orion` | v1.1 | - | - *Predictive aiming*, memperkirakan seberapa besar perputaran *gun* berdasarkan kalkulasi prediksi pergerakan musuh selanjutnya.<br>- Dynamic fire size, mengoptimalkan *damage* yang diberikan dari *bullet* secara dinamis berdasarkan jarak ke musuh.<br>- *Hitrate offset correction*, meminimalisir *miss hitrate bullet* ke bot musuh yang sedang bergerak. |

<!-- | **Alternatif 1** | `[Nama Bot 1]` | `src/alternative-bot/alt-bot-1` | `[Versi]` | `[Isi Tag]` | - `[Contoh: Agresif mendekat ke musuh terlemah (minimum energy)]`<br>- `[Contoh: Menembak dengan power maksimal setiap saat]` |
| **Alternatif 2** | `[Nama Bot 2]` | `src/alternative-bot/alt-bot-2` | `[Versi]` | `[Isi Tag]` | - `[Contoh: Menghindar ke arah ruang kosong terbesar (Surfing)]`<br>- `[Contoh: Menembak hanya jika probabilitas hit di atas 80%]` |
| **Alternatif 3** | `[Nama Bot 3]` | `src/alternative-bot/alt-bot-3` | `[Versi]` | `[Isi Tag]` | - `[Contoh: Strategi diam (Sniper) dan hanya bergerak jika ditembak]`<br>- `[Contoh: Memilih target yang paling dekat dengan posisi saat ini]` | -->

## Requirement

- [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) (saat ini menggunakan SDK 10.0.203)
- [Robocode Tank Royale GUI v0.30.0](https://github.com/Ariel-HS/tubes1-if2211-starter-pack/blob/main/robocode-tankroyale-gui-0.30.0.jar)

## Instalasi dan Cara Build

1. Download dan instal seluruh [requirement](#requirement) yang dibutuhkan.
2. Clone repository ini ke komputer lokal kamu melalui terminal.

    ```bash
    git clone https://github.com/N16Akbar/Tubes_BinTankLima
    ```

3. Jalankan aplikasi Robocode Tank Royale GUI yang sudah di-download sebelumnya.

    ```bash
    java -jar .\robocode-tankroyale-gui-0.30.0.jar
    ```

4. Buka menu **Config > Bot Root Directories** pada GUI Robocode (atau tekan shortcut `Ctrl + D`). Tambahkan direktori bot dengan melakukan *select* pada folder `src/main-bot`. Pastikan path yang dipilih berhenti di `main-bot` dan JANGAN masuk sampai ke folder `Orion` agar bot dapat terbaca oleh sistem.
5. Mulai pertempuran dengan membuka menu **Battle > Start Battle** (atau tekan `Ctrl + B`), lalu ikuti urutan berikut:
6. Pada panel **Bot Directories (local only)**, pilih bot yang ingin dimainkan lalu klik tombol **Boot ->**. Bot akan berpindah ke panel **Booted Bots (local only)**.
7. Tunggu beberapa saat hingga bot selesai di-compile dan muncul di panel **Joined Bots (local/remote)**.
8. Pilih bot yang sudah *joined* tersebut, lalu klik tombol **Add ->** atau **Add All ->** untuk memindahkannya ke dalam daftar **Selected Bots (battle participants)**.
9. Klik tombol **Start Battle** di bagian bawah untuk memulai permainan.

## Author

### Kelompok BinTank Lima

1. Imam Faris Rasyid (NIM. 124140004)
2. Mifthahul Rezki Akbar (NIM. 124140202)
3. Bimo Auliano (NIM. 124140198)
