<template>
  <div class="gurus">
    <ul>
      <li><b>Gurus</b></li>
      <li><br /></li>
      <li @click="hyperClicked"><em>Hyper Portfolio</em></li>
      <li><br /></li>
      <li v-if='guruList.length === 0'><b>No Gurus yet !!</b></li>
      <li v-for="g in guruList" @click="guruClicked(g.id, g.DisplayName, g.EndQuarterDate)">{{g.id + "/" + g.EndQuarterDate.substring(0,10) + "/" + g.DisplayName.substring(0,20).toLowerCase().replace(/\b\w/g, l => l.toUpperCase())}}</li>
      <li><input type="text" placeholder="Guru cik" v-model.number="guruid"></input></li>
      <li><button @click="addGuru(guruid)">Add</button><button @click="removeGuru(guruid)">Remove</button></li>
    </ul>
  </div>
</template>

<script>
export default {
  name: 'gurus',
  props: ['guruList'],
  data () {
    return {
      guruid: ''
    }
  },
  methods: {
    guruClicked: function (id, dname, ddate) { this.$emit('guruClicked', id, dname, ddate) },
    hyperClicked: function () { this.$emit('hyperClicked') },
    addGuru: function (id) { this.$emit('addGuru', id) },
    removeGuru: function (id) { this.$emit('removeGuru', id) }
  }
}
</script>

<style scoped>
button {
}

ul {
    list-style-type: none;
    margin: 0;
    padding: 0;
    background-color: #f1f1f1;
    border: 1px solid #555;
}

li {
    cursor: pointer;
    font-size: small;
    text-align: left;
    border-bottom: 1px solid #555;
}

li:last-child {
    border-bottom: none;
}

li:hover {
    background-color: #555;
    color: white;
}
.active {
    background-color: #4CAF50;
    color: white;
}
</style>
