# Tugas Besar IF25-21013 Strategi Algoritma

<div align="center">

## Pemanfaatan Algoritma Greedy dalam Pembuatan Bot Permainan Robocode Tank Royale

<!-- ![Robocode Banner](https://robocode.dev/images/robocode-logo.svg) -->
![Status](https://img.shields.io/badge/status-in--progress-red)
![C#](https://custom-icon-badges.demolab.com/badge/C%23-%23239120.svg?logo=cshrp&logoColor=white)

</div>

## Penjelasan Bot

| Status | Nama Bot | Lokasi Folder | Versi | Versi Tag | Algoritma Greedy |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Bot Utama** | **Orion** | `src/main-bot/Orion` | v1.1 | - | - *Predictive aiming*, memperkirakan seberapa besar perputaran *gun* berdasarkan kalkulasi prediksi pergerakan musuh selanjutnya.<br>- *Dynamic fire size*, mengoptimalkan *damage* yang diberikan dari *bullet* secara dinamis berdasarkan jarak ke musuh.<br>- *Hitrate offset correction*, meminimalisir *miss hitrate bullet* ke bot musuh yang sedang bergerak. |
| ... | ... | ... | ... | ... | ... |

## Requirement

- [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) (saat ini menggunakan SDK 10.0.203)
- [Robocode Tank Royale GUI v0.30.0](https://github.com/Ariel-HS/tubes1-if2211-starter-pack/blob/main/robocode-tankroyale-gui-0.30.0.jar)

## Instalasi dan Cara Build

1. *Download* dan instal seluruh [requirement](#requirement) yang dibutuhkan.
2. *Clone repository* ini ke komputer melalui terminal.

    ```bash
    git clone https://github.com/N16Akbar/Tubes_BinTankLima
    ```

3. Jalankan aplikasi Robocode Tank Royale GUI yang sudah di-download sebelumnya.

    ```bash
    java -jar .\robocode-tankroyale-gui-0.30.0.jar
    ```

4. Buka menu **Config > Bot Root Directories** pada GUI Robocode (`Ctrl + D`). Tambahkan direktori bot dengan klik tombol **Add**, lalu *select* pada folder `src/main-bot`. Pastikan path yang dipilih berhenti di `main-bot` dan **JANGAN** masuk sampai ke folder `Orion` agar bot dapat terbaca oleh sistem.
5. Konfigurasi *battle* dengan membuka menu **Battle > Start Battle** (`Ctrl + B`).
6. Pada panel **Bot Directories (local only)**, pilih bot yang ingin dimainkan lalu klik tombol **Boot ->**. Bot akan berpindah ke panel **Booted Bots (local only)**.
7. Tunggu beberapa saat hingga bot selesai di-*compile* dan muncul di panel **Joined Bots (local/remote)**.
8. Pilih bot yang sudah di dalam panel **Joined Bots (local/remote)**, lalu klik tombol **Add ->** atau **Add All ->** untuk memindahkannya ke dalam daftar **Selected Bots (battle participants)**.
9. Klik tombol **Start Battle** di bagian bawah untuk memulai permainan.

## Author

1. Imam Faris Rasyid (NIM. 124140004)
2. Mifthahul Rezki Akbar (NIM. 124140202)
3. Bimo Auliano (NIM. 124140198)
