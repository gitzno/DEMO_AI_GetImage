<template>
  <main>
    <ShoeSizeExtractor />
  </main>
</template>

<script setup>
// Import component bạn vừa tạo
import ShoeSizeExtractor from './components/ShoeSizeExtractor.vue'
</script>

<style>
/* CSS Reset cơ bản để web không bị viền trắng xung quanh */
body {
  margin: 0;
  padding: 0;
  background-color: #f8f9fa; /* Màu nền xám nhạt đồng bộ với component */
}
</style>
