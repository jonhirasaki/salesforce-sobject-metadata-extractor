# 🚀 Salesforce Metadata Extractor

This application extracts **Salesforce metadata** by querying **sObject descriptions** and their **child relationships**. The extracted data is structured as a **JSON hierarchy**, with child relationships organized as a **map (dictionary)**.

---

## 📌 Features

✅ Fetches metadata for **any Salesforce sObject**\
✅ Extracts **child relationships** and **nests them hierarchically**\
✅ **Prevents infinite recursion** on self-referencing objects\
✅ **Supports API error handling** (including **404 Not Found**)\
✅ Configurable **depth level** and **child relationship limits**\
✅ Outputs structured **JSON**

---

## 📂 Project Structure

```
/SalesforceMetadataExtractor
│── /src
│   ├── Program.cs                # Main entry point
│   ├── SalesforceDataProcessor.cs # Handles API calls and JSON processing
│   └── config.json                # Configuration file
│─ README.md                      # Documentation
│─ final_output.json               # Output JSON file
```

---

## 🚀 **Getting Started**

### **1️⃣ Prerequisites**

- Install **.NET 8 SDK** (or later) 👉 [Download](https://dotnet.microsoft.com/en-us/download)
- A **Salesforce API Access Token**
- A **Salesforce instance URL** (`https://your-instance.salesforce.com`)

---

### **2️⃣ Installation**

1. **Clone the repository**

   ```sh
   git clone https://github.com/your-repo/salesforce-metadata-extractor.git
   cd salesforce-metadata-extractor
   ```

2. **Open the project in VS Code**

   ```sh
   code .
   ```

3. **Restore dependencies**

   ```sh
   dotnet restore
   ```

---

### **3️⃣ Configuration**

Before running, update `` with your Salesforce credentials and query parameters:

#### **🔧 **``** Example**

```json
{
  "InstanceUrl": "https://your-salesforce-instance.salesforce.com",
  "AccessToken": "your-oauth-access-token",
  "InitialSObject": "Opportunity",
  "DepthLevel": 2,
  "MaxChildRelationships": 3,
  "ApiVersion": "v59.0",
  "OutputFilePath": "final_output.json"
}
```

| Config Key                | Description                                                          |
| ------------------------- | -------------------------------------------------------------------- |
| **InstanceUrl**           | Your Salesforce instance URL                                         |
| **AccessToken**           | OAuth access token                                                   |
| **InitialSObject**        | Root sObject to start querying                                       |
| **DepthLevel**            | Max levels of child relationships to explore                         |
| **MaxChildRelationships** | Limits the number of child relationships per sObject (0 = unlimited) |
| **ApiVersion**            | Salesforce API version                                               |
| **OutputFilePath**        | Path where JSON output is saved                                      |

---

### **4️⃣ Running the Application**

1. **Run the app using the config file**:
   ```sh
   dotnet run config.json
   ```
   Or, if compiled:
   ```sh
   SalesforceMetadataExtractor.exe config.json
   ```

---

## 💜 **Example Output (**``**)**

```json
{
  "sObject": "Opportunity",
  "childRelationships": {
    "AccountPartner": {
      "field": "OpportunityId",
      "associateEntityType": "",
      "associateParentEntity": "",
      "childRelationships": {
        "AccountPartner": {
          "field": "ReversePartnerId",
          "associateEntityType": "",
          "associateParentEntity": ""
        }
      }
    },
    "ActionPlan": {
      "field": "TargetId",
      "associateEntityType": "",
      "associateParentEntity": "",
      "childRelationships": {
        "ActionPlanItem": {
          "field": "ActionPlanId",
          "associateEntityType": "",
          "associateParentEntity": ""
        }
      }
    }
  }
}
```

---

## 🛠️ **Troubleshooting**

### **🔍 Common Issues**

| Issue                         | Solution                                      |
| ----------------------------- | --------------------------------------------- |
| ❌ `401 Unauthorized`          | Check your **AccessToken** in `config.json`   |
| ❌ `404 Not Found`             | The **sObject does not exist** in Salesforce  |
| ❌ `429 Too Many Requests`     | API rate limit exceeded, wait before retrying |
| ❌ `500 Internal Server Error` | Salesforce server error, try again later      |

---

## 🤝 **Contributing**

Feel free to **fork** and **submit PRs**! 🚀

```sh
git checkout -b feature/your-feature
git commit -m "Added new feature"
git push origin feature/your-feature
```

---

## 🐝 **License**

MIT License. See [LICENSE](LICENSE) for details.

---

## 📞 **Contact**

For issues or feature requests, open an **issue** on GitHub or contact me at: 📧 [**jon.hirasaki@gmail.com**](mailto\:jon.hirasaki@gmail.com)\
🔗 [**GitHub Repo**](https://github.com/jonhirasaki/salesforce-metadata-extractor)

