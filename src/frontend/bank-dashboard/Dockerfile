FROM node:22-alpine AS build

WORKDIR /app

COPY src/frontend/bank-dashboard/package*.json ./
RUN npm ci

COPY src/frontend/bank-dashboard ./
RUN npm run build

FROM nginx:1.29-alpine AS runtime

COPY src/frontend/bank-dashboard/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/bank-dashboard/browser /usr/share/nginx/html

EXPOSE 80
