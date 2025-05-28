# Deployment Guide - CalorieTracker API on Render

This guide explains how to deploy your CalorieTracker.API backend to Render using Docker.

## Files Created for Deployment

- `Dockerfile` - Multi-stage Docker build for the .NET API
- `.dockerignore` - Optimizes build by excluding unnecessary files
- `appsettings.Production.json` - Production configuration
- `render.yaml` - Render deployment configuration
- `DEPLOYMENT.md` - This deployment guide

## Deployment Steps

### 1. Prepare Your Repository
Ensure all changes are committed and pushed to GitHub:
```bash
git add .
git commit -m "Add Docker deployment configuration"
git push origin main
```

### 2. Deploy on Render

#### Option A: Using render.yaml (Recommended)
1. Go to [Render Dashboard](https://dashboard.render.com/)
2. Click "New" → "Blueprint"
3. Connect your GitHub repository
4. Render will automatically detect the `render.yaml` file
5. Click "Apply" to create the service

#### Option B: Manual Setup
1. Go to [Render Dashboard](https://dashboard.render.com/)
2. Click "New" → "Web Service"
3. Connect your GitHub repository
4. Configure the service:
   - **Name**: `calorietracker-api`
   - **Environment**: `Docker`
   - **Dockerfile Path**: `./Dockerfile`
   - **Docker Context**: `.`
   - **Branch**: `main`

### 3. Configure Environment Variables

Add these environment variables in Render:

| Key | Value |
|-----|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:10000` |
| `ConnectionStrings__DefaultConnection` | `Data Source=/app/data/calorietracker.db` |

### 4. Configure Persistent Storage

For the SQLite database to persist across deployments:

1. In your Render service dashboard, go to "Settings"
2. Scroll to "Persistent Disks"
3. Add a disk:
   - **Name**: `calorietracker-data`
   - **Mount Path**: `/app/data`
   - **Size**: `1 GB` (free tier)

### 5. Update CORS for Production

After deployment, update the CORS configuration:

1. Note your Render service URL (e.g., `https://calorietracker-api.onrender.com`)
2. Update `appsettings.Production.json`:
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://your-frontend-domain.onrender.com",
      "https://your-actual-frontend-url.com"
    ]
  }
}
```
3. Commit and push the changes

## Important Notes

### Database
- Uses SQLite with persistent storage
- Database is automatically created on first deployment
- Data persists across deployments with the mounted disk

### Port Configuration
- Render assigns a dynamic port, but the app listens on port 10000
- The `ASPNETCORE_URLS` environment variable handles this

### Health Checks
- Render will check `/swagger` endpoint for health
- The API will be accessible at your Render URL + `/swagger`

### Free Tier Limitations
- Service may sleep after 15 minutes of inactivity
- 750 hours/month of runtime
- 1GB persistent disk storage

## Testing Your Deployment

1. Once deployed, visit: `https://your-service-name.onrender.com/swagger`
2. You should see the Swagger UI with your API documentation
3. Test the endpoints using the Swagger interface

## Troubleshooting

### Build Failures
- Check the build logs in Render dashboard
- Ensure all NuGet packages are restored
- Verify Dockerfile syntax

### Runtime Issues
- Check the service logs in Render dashboard
- Verify environment variables are set correctly
- Ensure the persistent disk is mounted

### Database Issues
- Check if the `/app/data` directory is writable
- Verify the connection string format
- Check Entity Framework logs

## Updating Your Deployment

To update your API:
1. Make changes to your code
2. Commit and push to GitHub
3. Render will automatically rebuild and deploy

## Custom Domain (Optional)

To use a custom domain:
1. Go to your service settings in Render
2. Add your custom domain
3. Update DNS records as instructed
4. Update CORS settings to include your custom domain

## Security Considerations

- The API uses HTTPS in production (handled by Render)
- CORS is configured for specific origins
- Database is stored on encrypted persistent storage
- Application runs as non-root user in container
