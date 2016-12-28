<template>
  <table>
    <thead>
      <tr>
        <th v-for="key in columns"
          @click="sortBy(key)"
          :class="{ active: sortKey == key }">
          {{ key | capitalize }}
          <span class="arrow" :class="sortOrders[key] > 0 ? 'asc' : 'dsc'">
          </span>
        </th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="entry in filteredData">
        <td v-for="key in columns">
          {{entry[key]}}
        </td>
      </tr>
    </tbody>
  </table>
</template>

<script>
export default {
  name: 'portfolio',
  props: {
    data: Array,
    columns: Array
  },
  data: function () {
    return {
      sortKey: '',
      sortOrders: {}
    }
  },
  computed: {
    filteredData: function () {
      var sortKey = this.sortKey
      var order = this.sortOrders[sortKey] || 1
      var data = this.data
      if (sortKey) {
        data = data.slice().sort(function (a, b) {
          // Perhas the below is right. Attempt to manage the cases where one value is not present
          var as = a[sortKey]
          var bs = b[sortKey]
          if (as && bs) {
            var an = as.toString().replace(/,/g, '') // remove commas from formatted number columns
            var bn = bs.toString().replace(/,/g, '')
            var ak = !isNaN(parseFloat(an)) ? parseFloat(an) : as // if numbers, treat as numbers, othws bring columns back
            var bk = !isNaN(parseFloat(bn)) ? parseFloat(bn) : bs
            return (ak === bk ? 0 : ak > bk ? 1 : -1) * order
          } else if (as) {
            return 1 * order
          } else if (bs) {
            return -1 * order
          } else {
            return 0
          }
        })
      }
      return data
    }
  },
  filters: {
    capitalize: function (str) {
      return str.charAt(0).toUpperCase() + str.slice(1)
    }
  },
  methods: {
    sortBy: function (key) {
      this.sortKey = key
      this.sortOrders[key] = this.sortOrders[key] * -1
    }
  },
  created: function () {
    var sortOrders = {}
    this.columns.forEach(function (key) {
      sortOrders[key] = 1
    })
    this.sortOrders = sortOrders
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>

table {
  border: 2px solid #42b983;
  border-radius: 3px;
  background-color: #fff;
}

th {
  background-color: #42b983;
  color: rgba(255,255,255,0.66);
  cursor: pointer;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
  user-select: none;
}

td {
  background-color: #f9f9f9;
}

th, td {
  font-size: small;
  text-align: left;
}

th.active {
  color: #fff;
}

th.active .arrow {
  opacity: 1;
}

.arrow {
  display: inline-block;
  vertical-align: middle;
  width: 0;
  height: 0;
  margin-left: 5px;
  opacity: 0.66;
}

.arrow.asc {
  border-left: 4px solid transparent;
  border-right: 4px solid transparent;
  border-bottom: 4px solid #fff;
}

.arrow.dsc {
  border-left: 4px solid transparent;
  border-right: 4px solid transparent;
  border-top: 4px solid #fff;
}
</style>
