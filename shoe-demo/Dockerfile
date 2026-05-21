# Giai đoạn 1: Build project (Sử dụng Node.js để build)
FROM node:20-alpine AS build-stage
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# Giai đoạn 2: Serve project (Sử dụng Nginx để phục vụ file tĩnh)
FROM nginx:stable-alpine AS production-stage
# Copy các file đã build từ giai đoạn 1 sang thư mục mặc định của Nginx
COPY --from=build-stage /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]