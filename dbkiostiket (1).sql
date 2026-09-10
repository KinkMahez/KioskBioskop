-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Waktu pembuatan: 07 Agu 2026 pada 15.52
-- Versi server: 10.4.32-MariaDB
-- Versi PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `dbkiostiket`
--

-- --------------------------------------------------------

--
-- Struktur dari tabel `jadwal_tayang`
--

CREATE TABLE `jadwal_tayang` (
  `Jadwal_ID` int(11) NOT NULL,
  `Film_ID` int(11) NOT NULL,
  `Studio_ID` int(11) NOT NULL,
  `Tanggal_Tayang` date NOT NULL,
  `Jam_Mulai` time NOT NULL,
  `Jam_Selesai` time NOT NULL,
  `Harga_Tiket` decimal(10,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Struktur dari tabel `master_film`
--

CREATE TABLE `master_film` (
  `Film_ID` int(11) NOT NULL,
  `Judul` varchar(100) NOT NULL,
  `Sinopsis` text DEFAULT NULL,
  `Genre` varchar(50) DEFAULT NULL,
  `Durasi_Menit` int(11) NOT NULL,
  `Rating_Usia` varchar(10) DEFAULT 'SU',
  `Poster_Path` varchar(255) DEFAULT NULL,
  `Status_Tayang` varchar(20) DEFAULT 'NOW_SHOWING'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data untuk tabel `master_film`
--

INSERT INTO `master_film` (`Film_ID`, `Judul`, `Sinopsis`, `Genre`, `Durasi_Menit`, `Rating_Usia`, `Poster_Path`, `Status_Tayang`) VALUES
(1, 'alil', 'sd', 'ads', 2, 'Coming Soo', 'C:\\Users\\M4hez\\Downloads\\spider-man-brand-3840x2160-26882.jpg', 'R13+');

-- --------------------------------------------------------

--
-- Struktur dari tabel `master_kursi`
--

CREATE TABLE `master_kursi` (
  `Kursi_ID` int(11) NOT NULL,
  `Studio_ID` int(11) NOT NULL,
  `Kode_Baris` varchar(5) NOT NULL,
  `Nomor_Kolom` int(11) NOT NULL,
  `Label_Kursi` varchar(10) NOT NULL,
  `Status_Fisik` varchar(20) DEFAULT 'ACTIVE'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Struktur dari tabel `master_studio`
--

CREATE TABLE `master_studio` (
  `Studio_ID` int(11) NOT NULL,
  `Nama_Studio` varchar(50) NOT NULL,
  `Tipe_Studio` varchar(20) DEFAULT 'REGULAR',
  `Jumlah_Baris` int(11) NOT NULL,
  `Jumlah_Kolom` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Struktur dari tabel `transaksi`
--

CREATE TABLE `transaksi` (
  `Transaksi_ID` int(11) NOT NULL,
  `Kode_Transaksi` varchar(50) NOT NULL,
  `Kiosk_ID` varchar(20) DEFAULT 'KIOSK-01',
  `Waktu_Transaksi` datetime DEFAULT current_timestamp(),
  `Total_Tiket` int(11) NOT NULL,
  `Total_Bayar` decimal(10,2) NOT NULL,
  `Status_Transaksi` varchar(20) DEFAULT 'PENDING'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Struktur dari tabel `transaksi_detail`
--

CREATE TABLE `transaksi_detail` (
  `Detail_ID` int(11) NOT NULL,
  `Transaksi_ID` int(11) NOT NULL,
  `Jadwal_ID` int(11) NOT NULL,
  `Kursi_ID` int(11) NOT NULL,
  `Harga_Satuan` decimal(10,2) NOT NULL,
  `Kode_Tiket_QR` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Struktur dari tabel `users`
--

CREATE TABLE `users` (
  `User_ID` int(11) NOT NULL,
  `Username` varchar(50) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `Role` varchar(20) DEFAULT 'SUPERVISOR'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data untuk tabel `users`
--

INSERT INTO `users` (`User_ID`, `Username`, `Password`, `Role`) VALUES
(2, 'alil', 'fd917410cf1a2c0652f506514df464c5', 'admin');

--
-- Indexes for dumped tables
--

--
-- Indeks untuk tabel `jadwal_tayang`
--
ALTER TABLE `jadwal_tayang`
  ADD PRIMARY KEY (`Jadwal_ID`),
  ADD KEY `Film_ID` (`Film_ID`),
  ADD KEY `Studio_ID` (`Studio_ID`);

--
-- Indeks untuk tabel `master_film`
--
ALTER TABLE `master_film`
  ADD PRIMARY KEY (`Film_ID`);

--
-- Indeks untuk tabel `master_kursi`
--
ALTER TABLE `master_kursi`
  ADD PRIMARY KEY (`Kursi_ID`),
  ADD KEY `Studio_ID` (`Studio_ID`);

--
-- Indeks untuk tabel `master_studio`
--
ALTER TABLE `master_studio`
  ADD PRIMARY KEY (`Studio_ID`);

--
-- Indeks untuk tabel `transaksi`
--
ALTER TABLE `transaksi`
  ADD PRIMARY KEY (`Transaksi_ID`),
  ADD UNIQUE KEY `Kode_Transaksi` (`Kode_Transaksi`);

--
-- Indeks untuk tabel `transaksi_detail`
--
ALTER TABLE `transaksi_detail`
  ADD PRIMARY KEY (`Detail_ID`),
  ADD UNIQUE KEY `Kode_Tiket_QR` (`Kode_Tiket_QR`),
  ADD KEY `Transaksi_ID` (`Transaksi_ID`),
  ADD KEY `Jadwal_ID` (`Jadwal_ID`),
  ADD KEY `Kursi_ID` (`Kursi_ID`);

--
-- Indeks untuk tabel `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`User_ID`),
  ADD UNIQUE KEY `Username` (`Username`);

--
-- AUTO_INCREMENT untuk tabel yang dibuang
--

--
-- AUTO_INCREMENT untuk tabel `jadwal_tayang`
--
ALTER TABLE `jadwal_tayang`
  MODIFY `Jadwal_ID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT untuk tabel `master_film`
--
ALTER TABLE `master_film`
  MODIFY `Film_ID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT untuk tabel `master_kursi`
--
ALTER TABLE `master_kursi`
  MODIFY `Kursi_ID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT untuk tabel `master_studio`
--
ALTER TABLE `master_studio`
  MODIFY `Studio_ID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT untuk tabel `transaksi`
--
ALTER TABLE `transaksi`
  MODIFY `Transaksi_ID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT untuk tabel `transaksi_detail`
--
ALTER TABLE `transaksi_detail`
  MODIFY `Detail_ID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT untuk tabel `users`
--
ALTER TABLE `users`
  MODIFY `User_ID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- Ketidakleluasaan untuk tabel pelimpahan (Dumped Tables)
--

--
-- Ketidakleluasaan untuk tabel `jadwal_tayang`
--
ALTER TABLE `jadwal_tayang`
  ADD CONSTRAINT `jadwal_tayang_ibfk_1` FOREIGN KEY (`Film_ID`) REFERENCES `master_film` (`Film_ID`),
  ADD CONSTRAINT `jadwal_tayang_ibfk_2` FOREIGN KEY (`Studio_ID`) REFERENCES `master_studio` (`Studio_ID`);

--
-- Ketidakleluasaan untuk tabel `master_kursi`
--
ALTER TABLE `master_kursi`
  ADD CONSTRAINT `master_kursi_ibfk_1` FOREIGN KEY (`Studio_ID`) REFERENCES `master_studio` (`Studio_ID`) ON DELETE CASCADE;

--
-- Ketidakleluasaan untuk tabel `transaksi_detail`
--
ALTER TABLE `transaksi_detail`
  ADD CONSTRAINT `transaksi_detail_ibfk_1` FOREIGN KEY (`Transaksi_ID`) REFERENCES `transaksi` (`Transaksi_ID`) ON DELETE CASCADE,
  ADD CONSTRAINT `transaksi_detail_ibfk_2` FOREIGN KEY (`Jadwal_ID`) REFERENCES `jadwal_tayang` (`Jadwal_ID`),
  ADD CONSTRAINT `transaksi_detail_ibfk_3` FOREIGN KEY (`Kursi_ID`) REFERENCES `master_kursi` (`Kursi_ID`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
