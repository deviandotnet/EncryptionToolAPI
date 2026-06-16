# Encryption Tool API

## Purpose
The Encryption Tool API is a centralized cryptographic service designed to be consumed by various internal and external web applications. Its primary purpose is to offload the complexity of data encryption and decryption from individual consumer applications into a single, highly secure, and easily maintainable service.

## Goal
The goal of this project is to provide a unified endpoint for securing sensitive data before it is stored in consumer databases. By utilizing this service, we achieve:
1. **Data Integrity:** Prevent tampered data by using authenticated encryption (e.g., AES-GCM) so modifications to ciphertext are immediately detected.
2. **Centralized Key Management:** Keep encryption keys in a single secure location, rather than scattering them across multiple web apps, significantly reducing the attack surface.
3. **Consistency:** Ensure all applications use the same strong, standard-compliant cryptographic algorithms.
4. **Simplicity without Over-engineering:** Deliver a focused, performant API strictly responsible for cryptographic operations without bloating it into a massive monolithic service.

## Getting Started
(To be updated with .NET 8 CLI commands for running the project once the initial code is generated.)
