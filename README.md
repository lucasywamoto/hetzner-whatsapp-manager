# Hetzner WhatsApp Manager

A .NET Web API that allows you to manage your Hetzner Cloud servers via WhatsApp messages using Twilio.

## Features

- List all servers with status
- Get detailed server information
- Power on/off servers
- Graceful shutdown
- Reboot servers
- Phone number whitelist for security

## Commands

| Command | Description |
|---------|-------------|
| `list` | List all servers |
| `status <name\|id>` | Get server details |
| `start <name\|id>` | Power on server |
| `stop <name\|id>` | Force power off |
| `shutdown <name\|id>` | Graceful shutdown |
| `reboot <name\|id>` | Reboot server |
| `help` | Show help message |

## Setup

### 1. Hetzner API Token

1. Go to [Hetzner Cloud Console](https://console.hetzner.cloud/)
2. Select your project
3. Go to Security > API Tokens
4. Generate a new token with Read & Write permissions

### 2. Twilio WhatsApp Setup

1. Create a [Twilio account](https://www.twilio.com/)
2. Go to Messaging > Try it out > Send a WhatsApp message
3. Follow the sandbox setup instructions
4. Note your Account SID, Auth Token, and WhatsApp number

### 3. Configuration

Update `appsettings.json` or use environment variables/user secrets:

```json
{
  "Hetzner": {
    "ApiToken": "your-hetzner-api-token"
  },
  "Twilio": {
    "AccountSid": "your-twilio-account-sid",
    "AuthToken": "your-twilio-auth-token",
    "WhatsAppNumber": "+14155238886"
  },
  "AllowedPhoneNumbers": ["+1234567890"]
}
```

For production, use user secrets:
```bash
dotnet user-secrets set "Hetzner:ApiToken" "your-token"
dotnet user-secrets set "Twilio:AccountSid" "your-sid"
dotnet user-secrets set "Twilio:AuthToken" "your-token"
```

### 4. Expose Webhook

For local development, use ngrok:
```bash
ngrok http 5000
```

Configure the Twilio webhook URL:
`https://your-ngrok-url/api/webhook/twilio`

### 5. Run

```bash
cd src/HetznerWhatsApp.Api
dotnet run
```

## Deployment

### Deploy to Render.com (FREE)

1. **Push to GitHub**
   ```bash
   git init
   git add .
   git commit -m "Initial commit"
   git remote add origin https://github.com/yourusername/hetzner-whatsapp.git
   git push -u origin main
   ```

2. **Create Render Account**
   - Go to [render.com](https://render.com) and sign up

3. **Create New Web Service**
   - Click "New +" → "Web Service"
   - Connect your GitHub repository
   - Render will auto-detect the `render.yaml` configuration

4. **Set Environment Variables**
   In Render dashboard, add:
   - `Hetzner__ApiToken` - Your Hetzner API token
   - `Twilio__AccountSid` - Your Twilio Account SID
   - `Twilio__AuthToken` - Your Twilio Auth Token
   - `AllowedPhoneNumbers__0` - Your phone number (e.g., +1234567890)

5. **Deploy**
   - Render will automatically build and deploy
   - Your app URL: `https://hetzner-whatsapp.onrender.com`

6. **Configure Twilio Webhook**
   - Go to Twilio Console → Messaging → Settings
   - Set webhook URL: `https://your-app.onrender.com/api/webhook/twilio`

### Other Deployment Options

- **Azure App Service** - Free tier available
- **Railway.app** - $5/month free credit
- **fly.io** - Free tier with 3 VMs
- **Docker** - Use included Dockerfile for any container platform

## Security

- Always use HTTPS in production
- Configure `AllowedPhoneNumbers` to restrict access
- Store secrets securely (Azure Key Vault, AWS Secrets Manager, etc.)
- Consider implementing Twilio request validation

## Architecture

```
WhatsApp User
     │
     ▼
Twilio WhatsApp API
     │
     ▼
Webhook Controller ──► Command Handler ──► Hetzner Service
     │                                           │
     ▼                                           ▼
WhatsApp Service                          Hetzner Cloud API
     │
     ▼
WhatsApp User (response)
```
