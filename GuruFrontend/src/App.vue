<template>
  <div>
  <h1 v-if='showPort'>{{dname}}</h1>
  <h1 v-else>Hyper Portfolio</h1>
  <div id="app" class="flex-container">
    <gurus @guruClicked='guruClicked' @addGuru='addGuru' @removeGuru='removeGuru' @hyperClicked='hyperClicked' :guruList='guruList'></gurus>
    <portfolio v-if='showPort' :data='trades' :columns='tradeCols'></portfolio>
    <hyperPortfolio v-else :data='hyperTrades' :columns='hyperTradeCols'></hyperPortfolio>
  </div>
  </div>
</template>

<script>
import Gurus from './components/Gurus'
import Portfolio from './components/Portfolio'
import HyperPortfolio from './components/HyperPortfolio'

export default {
  name: 'app',
  components: {
    Gurus,
    Portfolio,
    HyperPortfolio
  },
  data () {
    return {
      // baseUri: '/api/portfolios',
      baseUri: 'https://gurufollower.azurewebsites.net/api/portfolios',
      collection: 'lucabol',
      keyPart: '?code=laenjSgq19DtzaypN/46w9OzDBabPHt6PMGfvf1a/BLba1VUgZUATg==',
      dname: '',
      ddate: {},
      showPort: true,
      guruList: [],
      trades: [],
      hyperTradeCols: ['Name', 'ClassTitle', 'Cusip', 'PutCall', 'PercOfPortfolio', 'NumberGurusOwning', 'NumberGurusBuying', 'NumberGurusSelling'],
      tradeCols: ['Name', 'ClassTitle', 'Cusip', 'Value', 'Shares', 'PutCall', 'Change', 'PercOfPortfolio', 'IsNew', 'IsSold', 'Price', 'Discretion'],
      hyperTrades: []
    }
  },
  computed: {
    hyperUri: function () { return this.baseUri + '/' + this.collection + this.keyPart },
    collectionUri: function () { return this.baseUri + '/' + this.collection + '/' }
  },
  methods: {
    guruClicked: function (id, dname, ddate) {
      this.loadPositions(id)
      this.dname = dname
      this.ddate = ddate
    },
    removeGuru: function (id) {
      // Sends the removal message and waits for the guru not to be present in the database to refresh UI.
      // TODO: refactor this func and next as they are similar
      this.$http.post(this.baseUri + this.keyPart, {collection: this.collection, groups: [], cik: id, remove: true}).then((response) => {
        var maxTries = 10
        var timeout = 250 // in msec
        var caller = this.$http
        var load = this.loadGurus
        var curi = this.collectionUri + id + this.keypart
        var f = function () {
          caller.get(curi).then((response) => {
            if (maxTries > 0) {
              maxTries = maxTries - 1
              window.setTimeout(f, timeout)
            } else {
              window.alert('Error removing guru with cik = ' + id + '. It was still in the database after the allotted removal time')
            }
          }, (error) => {
            // Can't load guru's portfolio. It must have been deleted. Reload everything.
            error
            load()
          })
        }
        window.setTimeout(f, timeout)
      })
    },
    addGuru: function (id) {
      // This tries for 2.5 sec every 250 ms to see if the guru as been inserted, if it had, then reloads
      // the list of gurus and shows the hyperportfolio
      this.$http.post(this.baseUri + this.keyPart, {collection: this.collection, groups: [], cik: id, remove: false}).then((response) => {
        var maxTries = 10
        var timeout = 250 // in msec
        var caller = this.$http
        var load = this.loadGurus
        var cUri = this.collectionUri + id + this.keyPart
        var f = function () {
          caller.get(cUri).then((response) => {
            load()
          }, (error) => {
            if (maxTries > 0) {
              maxTries = maxTries - 1
              window.setTimeout(f, timeout)
            } else {
              window.alert('Error adding guru with cik = ' + id + ' Error:' + error.toString())
            }
          })
        }
        window.setTimeout(f, timeout)
      })
    },
    hyperClicked: function () {
      this.loadHyperPositions()
    },
    formatTrades: function (trds) {
      trds.forEach(function (t) {
        try {
          t.Change = (t.Change * 100).toFixed(1) + '%'
          t.PercOfPortfolio = (t.PercOfPortfolio * 100).toFixed(1) + '%'
          t.IsNew = t.IsNew ? 'X' : ''
          t.IsSold = t.IsSold ? 'X' : ''
          t.Price = t.Price.toFixed(2)
          t.Shares = t.Shares.toLocaleString()
          t.Value = t.Value.toLocaleString()
        } catch (err) {
          window.alert('Guru with no positions? ' + err.toString())
        }
      })
      return trds
    },
    loadPositions: function (cik) {
      this.showPort = true
      if (cik) {
        this.$http.get(this.collectionUri + cik + this.keyPart).then((response) => {
          var port = response.body
          this.trades = this.formatTrades(port.Positions)
        })
      }
    },
    formatHyperTrades: function (trds) {
      trds.forEach(function (t) {
        t.PercOfPortfolio = (t.PercOfPortfolio * 100).toFixed(1) + '%'
      })
      return trds
    },
    loadHyperPositions: function () {
      this.showPort = false
      this.$http.get(this.hyperUri).then((response) => {
        var port = response.body
        this.hyperTrades = this.formatHyperTrades(port.Positions)
      })
    },
    loadGurus: function () {
      this.$http.get(this.collectionUri + 'gurus' + this.keyPart).then((response) => {
        this.guruList = response.body
        this.loadHyperPositions()
      })
    }
  },
  created: function () {
    this.trades = []
    this.guruList = []
    this.loadGurus()
  }
}
</script>

<style>
#app {
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}
h1 {
  text-align: center;
}
.flex-container {
    display: -webkit-flex;
    display: flex;
}
</style>
