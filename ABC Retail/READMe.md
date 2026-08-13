# ABC Retail Management Hub

An enterprise-grade ASP.NET Core MVC web application integrated with **Azure Storage Services** (Tables, Blobs, Queues, and Files) to manage retail operations, inventory dispatches, product media, customer accounts, and administrative audit logging.

---

## Architecture & Azure Storage Services Overview

This application leverages four core Azure Cloud Storage services to handle diverse data requirements:

* **Azure Table Storage (`CustomerProfile` & `Product` Entities):** Provides NoSQL key-value storage for high-throughput customer account records and product catalog metadata.
* **Azure Blob Storage (`product-images` Container):** Stores high-resolution multimedia product images, seamlessly serving media content via direct Blob URLs.
* **Azure Queue Storage (`order-processing` Queue):** Facilitates real-time, asynchronous transaction messaging and event logging for stock adjustments, dispatches, and image uploads.
* **Azure Files (`system-logs` File Share):** Offers fully managed SMB file shares in the cloud to store uploaded compliance contracts, business documents, and automatically generated live system audit logs.

---

## Features

* **Centralized Dashboard:** Real-time metrics tracking total saved profiles, listed products, active queue events, and stored log files, alongside a live Azure Queue activity feed.
* **Customer Profile Management:** Full CRUD operations for customer records stored in Azure Tables with automated queue event logging upon creation, editing, or deletion.
* **Product Catalog & Stock Management:** Upload product listings with images to Azure Blob Storage, update pricing, and adjust stock quantities with automated inventory queue triggers.
* **Order & Inventory Queue Manager:** View active processing messages from Azure Queue Storage and manually inject or clear custom order/inventory event payloads.
* **System Audit & Document Storage:** Upload business contracts (.pdf, .doc, .txt) to Azure Files Share and export live system audit logs capturing UTC-timestamped user activity.

---

## Tech Stack & Prerequisites

### Frameworks & Libraries
* **Framework:** ASP.NET Core MVC 10.0
* **Frontend:** HTML5, CSS3, Bootstrap 5, FontAwesome 6
* **Language:** C#

### Azure SDK Packages
* `Azure.Data.Tables`
* `Azure.Storage.Blobs`
* `Azure.Storage.Queues`
* `Azure.Storage.Files.Shares`

---

## Getting Started & Configuration

### 1. Clone the Repository
```bash
git clone [https://github.com/ST10225793/ABC_Retail.git](https://github.com/ST10225793/ABC_Retail.git)
cd ABC_Retail