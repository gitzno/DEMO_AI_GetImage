<template>
  <div
    class="page-wrapper d-flex align-items-center justify-content-center min-vh-100 bg-light-gradient font-sans"
  >
    <div class="app-container p-4 p-md-5 rounded-4 shadow-lg bg-white">
      <div class="text-center mb-4">
        <div
          class="icon-box text-primary rounded-circle mb-3 mx-auto d-flex align-items-center justify-content-center"
        >
          <img src="/src/assets/White.svg" style="width: 100px" />
        </div>
        <h2 class="h3 fw-bold text-dark mb-1">TDMK Shoe Size Scanner</h2>
        <p class="text-muted small">Tải lên tối đa 5 ảnh mác giày để trích xuất tự động</p>
      </div>

      <div
        class="upload-zone position-relative mb-4 rounded-3"
        :class="{ 'is-dragover': isDragging }"
        @dragover.prevent="isDragging = true"
        @dragleave.prevent="isDragging = false"
        @drop.prevent="handleDrop"
        @click="triggerFileInput"
      >
        <div class="text-center py-4">
          <i class="fs-1 text-secondary mb-2 opacity-50">📁</i>
          <h6 class="fw-semibold mb-1">Kéo thả hoặc nhấn để chọn ảnh</h6>
          <span class="text-secondary small">JPG, PNG, WEBP (Tối đa 5 file)</span>
        </div>
        <input
          type="file"
          ref="fileInputRef"
          multiple
          accept="image/*"
          class="d-none"
          @change="handleFileSelect"
        />
      </div>

      <TransitionGroup
        name="list"
        tag="ul"
        class="list-group list-group-flush mb-4 rounded-3 border overflow-hidden"
        v-if="selectedFiles.length > 0"
      >
        <li
          v-for="(file, index) in selectedFiles"
          :key="file.name"
          class="list-group-item d-flex justify-content-between align-items-center border-0 bg-light mb-1 rounded"
        >
          <div class="d-flex align-items-center text-truncate pe-3">
            <span class="me-2">📄</span>
            <span class="text-truncate small fw-medium">{{ file.name }}</span>
          </div>
          <span class="badge bg-secondary rounded-pill fw-normal"
            >{{ (file.size / 1024).toFixed(1) }} KB</span
          >
        </li>
      </TransitionGroup>

      <div class="d-flex flex-column flex-sm-row gap-3 mt-2">
        <button
          class="btn btn-modern btn-primary flex-grow-1 py-2 fw-semibold"
          :disabled="selectedFiles.length === 0 || isProcessing || cooldownTime > 0"
          @click="uploadAllImagesSequentially"
        >
          <span v-if="isProcessing" class="spinner-border spinner-border-sm me-2"></span>
          <span v-else-if="cooldownTime > 0">Vui lòng đợi {{ cooldownTime }}s...</span>
          <span v-else>✨ Phân Tích AI</span>
        </button>

        <button
          class="btn btn-modern flex-grow-1 py-2 fw-semibold"
          :class="
            results.length > 0 ? 'btn-success text-white shadow-success' : 'btn-light text-muted'
          "
          :disabled="results.length === 0 || isProcessing"
          @click="exportTableToCSV"
        >
          📥 Xuất CSV
        </button>
      </div>

      <Transition name="fade">
        <div v-if="results.length > 0" class="mt-5">
          <h6 class="fw-bold mb-3 text-dark d-flex align-items-center">
            <span class="me-2 text-success">✔️</span> Kết quả trích xuất
          </h6>
          <div class="table-responsive rounded-3 border">
            <table class="table table-hover table-borderless align-middle mb-0">
              <thead class="table-light border-bottom">
                <tr>
                  <th class="ps-3 text-secondary fw-semibold small">Tên Ảnh</th>
                  <th class="text-center text-secondary fw-semibold small">US</th>
                  <th class="text-center text-secondary fw-semibold small">UK</th>
                  <th class="text-center text-secondary fw-semibold small">FR</th>
                  <th class="text-center text-secondary fw-semibold small">JP</th>
                  <th class="text-center text-secondary fw-semibold small">CHN</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(item, index) in results" :key="index" class="border-bottom">
                  <td class="ps-3 fw-medium" :class="{ 'text-danger': !item.success }">
                    {{ item.fileName }}
                    <small
                      v-if="!item.success"
                      class="d-block text-danger opacity-75"
                      style="font-size: 0.7rem"
                      >Lỗi phân tích</small
                    >
                  </td>
                  <td class="text-center fw-bold text-primary">{{ item.data?.US ?? '-' }}</td>
                  <td class="text-center text-dark">{{ item.data?.UK ?? '-' }}</td>
                  <td class="text-center text-dark">{{ item.data?.FR ?? '-' }}</td>
                  <td class="text-center text-dark">{{ item.data?.JP ?? '-' }}</td>
                  <td class="text-center text-dark">{{ item.data?.CHN ?? '-' }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </Transition>
    </div>
  </div>
</template>

<script setup>
import { ref, onUnmounted } from 'vue'

// --- Khởi tạo các biến State ---
const fileInputRef = ref(null)
const selectedFiles = ref([])
const results = ref([])
const isProcessing = ref(false)
const isDragging = ref(false)

// Biến quản lý Cooldown (Chống spam)
const cooldownTime = ref(0)
const COOLDOWN_DURATION = 30 // Chờ 30 giây
let cooldownTimer = null

// --- Xử lý sự kiện file ---
const triggerFileInput = () => fileInputRef.value.click()

const handleFiles = (filesArray) => {
  if (filesArray.length > 5) {
    alert('Bạn chỉ được chọn tối đa 5 ảnh một lúc.')
    return
  }
  selectedFiles.value = filesArray
  results.value = [] // Làm mới bảng kết quả
}

const handleFileSelect = (event) => {
  handleFiles(Array.from(event.target.files))
  event.target.value = '' // Reset input để có thể chọn lại file cũ
}

const handleDrop = (event) => {
  isDragging.value = false
  const files = Array.from(event.dataTransfer.files).filter((f) => f.type.startsWith('image/'))
  handleFiles(files)
}

// --- Xử lý Timer đếm ngược ---
const startCooldown = () => {
  cooldownTime.value = COOLDOWN_DURATION
  clearInterval(cooldownTimer)
  cooldownTimer = setInterval(() => {
    cooldownTime.value--
    if (cooldownTime.value <= 0) clearInterval(cooldownTimer)
  }, 1000)
}

onUnmounted(() => clearInterval(cooldownTimer)) // Dọn dẹp RAM khi thoát trang

// --- GỌI API TUẦN TỰ (Xong ảnh này mới gửi ảnh tiếp theo) ---
const uploadAllImagesSequentially = async () => {
  if (selectedFiles.value.length === 0 || cooldownTime.value > 0) return

  isProcessing.value = true
  results.value = []

  for (const file of selectedFiles.value) {
    const formData = new FormData()
    formData.append('image', file)

    try {
      // Gọi lên ASP.NET Core Backend
      const response = await fetch('http://apis.tdmk.vn/api/shoesize/extract', {
        method: 'POST',
        body: formData,
      })

      if (!response.ok) throw new Error('Lỗi Server hoặc API')

      const resData = await response.json()

      // Đẩy thẳng vào mảng results -> Giao diện sẽ render ngay dòng này
      results.value.push({ fileName: file.name, data: resData.data, success: true })
    } catch (error) {
      console.error(`Lỗi khi xử lý file ${file.name}:`, error)
      results.value.push({ fileName: file.name, data: null, success: false })
    }
  }

  isProcessing.value = false
  startCooldown() // Kích hoạt chặn click 30s
}

// --- Xuất CSV chuẩn UTF-8 Tiếng Việt ---
const exportTableToCSV = () => {
  if (results.value.length === 0) return

  const headers = ['Tên Ảnh', 'US', 'UK', 'FR', 'JP', 'CHN']
  const csvRows = [headers.join(',')]

  results.value.forEach((item) => {
    const d = item.data || {}
    const row = [
      `"${item.fileName}"`,
      `"${d.US ?? ''}"`,
      `"${d.UK ?? ''}"`,
      `"${d.FR ?? ''}"`,
      `"${d.JP ?? ''}"`,
      `"${d.CHN ?? ''}"`,
    ]
    csvRows.push(row.join(','))
  })

  const csvString = '\uFEFF' + csvRows.join('\n') // \uFEFF là BOM giúp Excel đọc đúng tiếng Việt
  const blob = new Blob([csvString], { type: 'text/csv;charset=utf-8;' })

  const link = document.createElement('a')
  link.href = URL.createObjectURL(blob)
  link.setAttribute(
    'download',
    `[TDMK] Bao_Cao_Size_Giay_${new Date().toISOString().slice(0, 10)}.csv`,
  )
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}
</script>

<style scoped>
/* Reset và Import Font/Bootstrap */
@import url('https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css');
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

.font-sans {
  font-family:
    'Inter',
    system-ui,
    -apple-system,
    sans-serif;
}
.bg-light-gradient {
  background: linear-gradient(135deg, #f5f7fa 0%, #e4e8f0 100%);
}
.app-container {
  width: 100%;
  max-width: 600px;
  transition: all 0.3s ease;
}

/* Icon Box */
.icon-box {
  width: 60px;
  height: 60px;
  font-size: 24px;
}

/* Upload Zone */
.upload-zone {
  border: 2px dashed #cbd5e1;
  background: #f8fafc;
  cursor: pointer;
  transition: all 0.2s ease-in-out;
}
.upload-zone:hover,
.upload-zone.is-dragover {
  border-color: #3b82f6;
  background: #eff6ff;
}

/* Nút bấm hiện đại */
.btn-modern {
  border-radius: 10px;
  transition: all 0.2s ease;
  border: none;
}
.btn-modern:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}
.shadow-success {
  box-shadow: 0 4px 15px rgba(25, 135, 84, 0.3) !important;
}

/* Hiệu ứng chuyển động mượt mà cho Vue */
.list-enter-active,
.list-leave-active {
  transition: all 0.3s ease;
}
.list-enter-from,
.list-leave-to {
  opacity: 0;
  transform: translateX(-20px);
}
.fade-enter-active,
.fade-leave-active {
  transition:
    opacity 0.4s ease,
    transform 0.4s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateY(10px);
}
</style>
